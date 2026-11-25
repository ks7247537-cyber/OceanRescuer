import math
import wave
import struct
import os
import random

SAMPLE_RATE = 44100

def create_wav(filename, samples):
    # Ensure -1.0 to 1.0 clamped and converted to 16-bit PCM
    os.makedirs(os.path.dirname(filename), exist_ok=True)
    with wave.open(filename, 'w') as wav_file:
        wav_file.setnchannels(1)  # Mono
        wav_file.setsampwidth(2)  # 16-bit
        wav_file.setframerate(SAMPLE_RATE)
        
        # Max normalization to prevent clipping while maximizing clarity
        max_val = max(abs(s) for s in samples) if samples else 1.0
        if max_val > 0.001:
            gain = 0.88 / max_val
        else:
            gain = 1.0
            
        data = bytearray()
        for s in samples:
            val = int(max(-32767, min(32767, s * gain * 32767.0)))
            data.extend(struct.pack('<h', val))
        wav_file.writeframes(data)
    print(f"Generated: {filename} ({len(samples)/SAMPLE_RATE:.2f}s, {len(data)} bytes)")

def note_freq(note_str):
    # e.g. C4=261.63, E4=329.63, G4=392.00, etc.
    notes = {'C': 0, 'C#': 1, 'Db': 1, 'D': 2, 'D#': 3, 'Eb': 3, 'E': 4,
             'F': 5, 'F#': 6, 'Gb': 6, 'G': 7, 'G#': 8, 'Ab': 8, 'A': 9, 'A#': 10, 'Bb': 10, 'B': 11}
    pitch = note_str[:-1]
    octave = int(note_str[-1])
    semitone = notes[pitch] + (octave - 4) * 12
    return 440.0 * (2.0 ** ((semitone - 9) / 12.0))

def marimba_tone(freq, duration, amp=1.0):
    num_samples = int(duration * SAMPLE_RATE)
    samples = [0.0] * num_samples
    decay_rate = 6.0 / duration
    for i in range(num_samples):
        t = i / SAMPLE_RATE
        env = math.exp(-decay_rate * t) * (1.0 - math.exp(-150.0 * t))
        # Fundamental + harmonic mallet strike
        s = math.sin(2.0 * math.pi * freq * t) * 0.7
        s += math.sin(2.0 * math.pi * freq * 2.0 * t) * 0.2 * math.exp(-decay_rate * 2.5 * t)
        s += math.sin(2.0 * math.pi * freq * 3.0 * t) * 0.1 * math.exp(-decay_rate * 4.0 * t)
        samples[i] = s * env * amp
    return samples

def synth_bgm_menu():
    # 8-second seamless loop in C Major (C - G - Am - F)
    # Tropical underwater cartoon kalimba / marimba
    duration = 8.0
    num_samples = int(duration * SAMPLE_RATE)
    mix = [0.0] * num_samples

    # Chord progression: C4, G3, A3, F3 (2 seconds each)
    chords = [
        ('C4', ['C4', 'E4', 'G4', 'C5']),
        ('G3', ['B3', 'D4', 'G4', 'B4']),
        ('A3', ['A3', 'C4', 'E4', 'A4']),
        ('F3', ['F3', 'A3', 'C4', 'F4']),
    ]
    
    # Bass pad (warm sub swell)
    for c_idx, (bass, _) in enumerate(chords):
        b_freq = note_freq(bass)
        start_t = c_idx * 2.0
        for i in range(int(2.0 * SAMPLE_RATE)):
            idx = int(start_t * SAMPLE_RATE) + i
            if idx >= num_samples: break
            t = i / SAMPLE_RATE
            # Soft envelope
            pad_env = math.sin(math.pi * (t / 2.0))
            mix[idx] += math.sin(2.0 * math.pi * b_freq * 0.5 * t) * 0.22 * pad_env
            # Undulating water tremolo
            tremolo = 1.0 + 0.15 * math.sin(2.0 * math.pi * 3.5 * t)
            mix[idx] += math.sin(2.0 * math.pi * b_freq * t) * 0.18 * pad_env * tremolo

    # Plucked melody / arpeggio pattern
    pattern = [
        # Bar 1 (C)
        (0.00, 'C4', 0.4), (0.25, 'E4', 0.4), (0.50, 'G4', 0.4), (0.75, 'C5', 0.4),
        (1.00, 'E4', 0.4), (1.25, 'G4', 0.4), (1.50, 'C5', 0.4), (1.75, 'G4', 0.3),
        # Bar 2 (G)
        (2.00, 'B3', 0.4), (2.25, 'D4', 0.4), (2.50, 'G4', 0.4), (2.75, 'B4', 0.4),
        (3.00, 'D4', 0.4), (3.25, 'G4', 0.4), (3.50, 'B4', 0.4), (3.75, 'D4', 0.3),
        # Bar 3 (Am)
        (4.00, 'A3', 0.4), (4.25, 'C4', 0.4), (4.50, 'E4', 0.4), (4.75, 'A4', 0.4),
        (5.00, 'C4', 0.4), (5.25, 'E4', 0.4), (5.50, 'A4', 0.4), (5.75, 'E4', 0.3),
        # Bar 4 (F)
        (6.00, 'F3', 0.4), (6.25, 'A3', 0.4), (6.50, 'C4', 0.4), (6.75, 'F4', 0.4),
        (7.00, 'A3', 0.4), (7.25, 'C4', 0.4), (7.50, 'E4', 0.4), (7.75, 'D4', 0.3),
    ]

    for start_t, note, note_dur in pattern:
        f = note_freq(note)
        tone = marimba_tone(f, note_dur * 1.5, amp=0.35)
        for i, s in enumerate(tone):
            idx = int(start_t * SAMPLE_RATE) + i
            if idx < num_samples:
                mix[idx] += s
            else:
                # Wrap around for seamless loop
                mix[idx % num_samples] += s

    # Ensure seamless zero-crossing at ends
    fade_len = int(0.05 * SAMPLE_RATE)
    for i in range(fade_len):
        w = i / fade_len
        mix[i] = mix[i] * w + mix[num_samples - fade_len + i] * (1.0 - w)
        mix[num_samples - fade_len + i] = mix[i]

    return mix

def synth_bgm_gameplay():
    # 12-second peaceful, thoughtful underwater puzzle atmosphere
    duration = 12.0
    num_samples = int(duration * SAMPLE_RATE)
    mix = [0.0] * num_samples

    # Chord progression: C - Am - F - G (3.0s each)
    chords = [
        ('C3', ['C4', 'G4', 'E5']),
        ('A2', ['A3', 'E4', 'C5']),
        ('F2', ['F3', 'C4', 'A4']),
        ('G2', ['G3', 'D4', 'B4']),
    ]

    for c_idx, (bass, arp) in enumerate(chords):
        b_freq = note_freq(bass)
        start_t = c_idx * 3.0
        # Warm ocean ambient drone
        for i in range(int(3.0 * SAMPLE_RATE)):
            idx = int(start_t * SAMPLE_RATE) + i
            if idx >= num_samples: break
            t = i / SAMPLE_RATE
            env = math.sin(math.pi * (t / 3.0))
            # Gentle filtered ocean swell
            mix[idx] += math.sin(2.0 * math.pi * b_freq * t) * 0.20 * env
            mix[idx] += math.sin(2.0 * math.pi * b_freq * 1.5 * t) * 0.12 * env
            # Subtle resonant bubble wobble
            mix[idx] += math.sin(2.0 * math.pi * b_freq * 2.0 * t) * 0.08 * env * (1.0 + 0.2 * math.sin(2.0 * math.pi * 2.5 * t))

        # Delicate harp/chime arpeggios
        for n_idx, note in enumerate(arp):
            f = note_freq(note)
            n_start = start_t + n_idx * 0.75
            tone = marimba_tone(f, 2.2, amp=0.28)
            for i, s in enumerate(tone):
                idx = int(n_start * SAMPLE_RATE) + i
                if idx < num_samples:
                    mix[idx] += s
                else:
                    mix[idx % num_samples] += s

    # Ensure seamless zero-crossing loop
    fade_len = int(0.05 * SAMPLE_RATE)
    for i in range(fade_len):
        w = i / fade_len
        mix[i] = mix[i] * w + mix[num_samples - fade_len + i] * (1.0 - w)
        mix[num_samples - fade_len + i] = mix[i]

    return mix

def synth_pipe_rotate():
    # Tactile cartoon rotation: short pitch sweep + wooden ratchet lock click
    duration = 0.18
    num_samples = int(duration * SAMPLE_RATE)
    samples = [0.0] * num_samples
    
    # 1. Whoosh sweep (650Hz down to 240Hz)
    for i in range(num_samples):
        t = i / SAMPLE_RATE
        # Exponential pitch drop
        f = 650.0 * math.exp(-7.0 * t) + 240.0
        env = math.sin(math.pi * (t / duration)) ** 1.2
        phase = 2.0 * math.pi * f * t
        samples[i] += math.sin(phase) * 0.45 * env

    # 2. Ratchet snap click at 0.10s
    click_start = int(0.10 * SAMPLE_RATE)
    click_len = int(0.06 * SAMPLE_RATE)
    for i in range(click_len):
        idx = click_start + i
        if idx >= num_samples: break
        t = i / SAMPLE_RATE
        click_env = math.exp(-80.0 * t)
        # Metallic/wood resonance click
        click = (math.sin(2.0 * math.pi * 880.0 * t) * 0.6 +
                 math.sin(2.0 * math.pi * 1450.0 * t) * 0.4)
        samples[idx] += click * click_env * 0.65

    return samples

def synth_win():
    # Joyful rescue victory fanfare (C5 - E5 - G5 - C6 - E6) + sparkling chime
    duration = 1.7
    num_samples = int(duration * SAMPLE_RATE)
    mix = [0.0] * num_samples

    notes = [
        (0.00, 'C5', 0.22, 0.5),
        (0.15, 'E5', 0.22, 0.55),
        (0.30, 'G5', 0.25, 0.6),
        (0.48, 'C6', 0.30, 0.7),
        (0.70, 'E6', 0.90, 0.8),
    ]

    for start_t, note, dur, amp in notes:
        f = note_freq(note)
        tone = marimba_tone(f, dur * 2.0, amp=amp)
        sparkle = marimba_tone(f * 2.0, dur * 2.0, amp=amp * 0.35)
        for i in range(len(tone)):
            idx = int(start_t * SAMPLE_RATE) + i
            if idx < num_samples:
                mix[idx] += tone[i] + sparkle[i]

    # Warm sustained victory chord at 0.70s
    chord = ['C5', 'G5', 'C6', 'E6']
    c_start = int(0.70 * SAMPLE_RATE)
    c_len = int(0.95 * SAMPLE_RATE)
    for i in range(c_len):
        idx = c_start + i
        if idx >= num_samples: break
        t = i / SAMPLE_RATE
        env = math.exp(-3.5 * t) * (1.0 - math.exp(-80.0 * t))
        vibrato = 1.0 + 0.08 * math.sin(2.0 * math.pi * 5.5 * t)
        for n in chord:
            f = note_freq(n)
            mix[idx] += math.sin(2.0 * math.pi * f * t * vibrato) * 0.15 * env

    return mix

def synth_fail():
    # Gentle cartoon fail: soft descending musical slide (E4 -> D4 -> C4 -> B3)
    duration = 1.1
    num_samples = int(duration * SAMPLE_RATE)
    mix = [0.0] * num_samples

    # Soft playful trombone wah-wah
    notes = [
        (0.00, 330.0, 0.25),
        (0.22, 294.0, 0.25),
        (0.44, 261.0, 0.30),
        (0.70, 220.0, 0.38),
    ]

    for start_t, f, dur in notes:
        n_len = int(dur * SAMPLE_RATE)
        for i in range(n_len):
            idx = int(start_t * SAMPLE_RATE) + i
            if idx >= num_samples: break
            t = i / SAMPLE_RATE
            # Wah envelope
            env = math.sin(math.pi * (t / dur)) * math.exp(-1.5 * t)
            # Warm soft wave
            s = math.sin(2.0 * math.pi * f * t) * 0.6 + math.sin(2.0 * math.pi * f * 2.0 * t) * 0.2
            mix[idx] += s * env * 0.55

    return mix

def synth_button_click():
    # Tactile cartoon bubble pop click (0.07s)
    duration = 0.07
    num_samples = int(duration * SAMPLE_RATE)
    samples = [0.0] * num_samples
    
    for i in range(num_samples):
        t = i / SAMPLE_RATE
        # Fast descending pitch 1200Hz -> 450Hz
        f = 1200.0 * math.exp(-35.0 * t) + 450.0
        env = math.exp(-55.0 * t)
        phase = 2.0 * math.pi * f * t
        s = math.sin(phase) + 0.3 * math.sin(phase * 2.0)
        samples[i] = s * env * 0.75

    return samples

def synth_water_flow():
    # Gentle rushing water stream with bubbling texture (2.2s)
    duration = 2.2
    num_samples = int(duration * SAMPLE_RATE)
    samples = [0.0] * num_samples

    # Generate filtered pink noise + bubbling sine modulators
    random.seed(42)
    b0 = b1 = b2 = b3 = b4 = b5 = b6 = 0.0
    
    for i in range(num_samples):
        t = i / SAMPLE_RATE
        # Envelope: soft fade in (0.3s) and fade out (0.4s)
        fade_in = min(1.0, t / 0.35)
        fade_out = min(1.0, (duration - t) / 0.45)
        env = fade_in * fade_out

        # Pink noise filter
        white = random.uniform(-1.0, 1.0)
        b0 = 0.99886 * b0 + white * 0.0555179
        b1 = 0.99332 * b1 + white * 0.0750759
        b2 = 0.96900 * b2 + white * 0.1538520
        b3 = 0.86650 * b3 + white * 0.3104856
        b4 = 0.55000 * b4 + white * 0.5329522
        b5 = -0.7616 * b5 - white * 0.0168980
        pink = b0 + b1 + b2 + b3 + b4 + b5 + b6 + white * 0.5362
        b6 = white * 0.115926

        # Resonant water bubble oscillations (frequencies 500Hz - 900Hz with 6Hz-14Hz flutter)
        flutter = math.sin(2.0 * math.pi * 8.0 * t) * 0.2 + math.sin(2.0 * math.pi * 14.0 * t) * 0.15
        bubble = (math.sin(2.0 * math.pi * (520.0 + flutter * 80.0) * t) * 0.25 +
                  math.sin(2.0 * math.pi * (740.0 - flutter * 60.0) * t) * 0.18)

        samples[i] = (pink * 0.08 + bubble * 0.35) * env

    return samples

def main():
    print("--- SYNTHESIZING ALL 7 CARTOON AUDIO ASSETS ---")
    assets = {
        'bgm_menu.wav': synth_bgm_menu(),
        'bgm_gameplay.wav': synth_bgm_gameplay(),
        'sfx_pipe_rotate.wav': synth_pipe_rotate(),
        'sfx_win.wav': synth_win(),
        'sfx_fail.wav': synth_fail(),
        'sfx_button_click.wav': synth_button_click(),
        'sfx_water_flow.wav': synth_water_flow(),
    }

    dirs = ['Assets/Audio', 'Assets/Resources/Audio']
    for d in dirs:
        os.makedirs(d, exist_ok=True)
        for name, data in assets.items():
            path = os.path.join(d, name)
            create_wav(path, data)

    print("All audio files successfully generated and deployed!")

if __name__ == '__main__':
    main()
