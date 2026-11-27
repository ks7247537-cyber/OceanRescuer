with open('Assets/Script/CreatureAbilityHUD.cs', 'r', encoding='utf-8') as f:
    text = f.read()

target1 = '''        // 4. Status Text & Availability Indicator
        if (abilityStatusText != null)
        {
            if (isPassive)
            {
                abilityStatusText.text = "PASSIVE";
                abilityStatusText.color = passiveColor;
            }
            else
            {
                abilityStatusText.text = hasCharges ? "READY" : "EMPTY";
                abilityStatusText.color = hasCharges ? readyColor : depletedColor;
            }
        }'''

replacement1 = '''        // 4. Status Text & Availability Indicator
        if (abilityStatusText != null)
        {
            if (isPassive)
            {
                abilityStatusText.text = "PASSIVE";
                abilityStatusText.color = passiveColor;
            }
            else
            {
                abilityStatusText.text = hasCharges ? "Ready" : "Empty";
                abilityStatusText.color = hasCharges ? readyColor : depletedColor;
            }
        }

        if (statusBadgeBackground != null)
        {
            statusBadgeBackground.color = (hasCharges || isPassive)
                ? _initialStatusBadgeColor
                : new Color(_initialStatusBadgeColor.r * 0.7f, _initialStatusBadgeColor.g * 0.7f, _initialStatusBadgeColor.b * 0.7f, 0.65f);
        }'''

target2 = '''            if (abilityPulseEffect != null)
            {
                abilityPulseEffect.color = new Color(_initialPulseColor.r, _initialPulseColor.g, _initialPulseColor.b, alpha);
                float s = Mathf.Lerp(0.98f, 1.05f, t);
                abilityPulseEffect.transform.localScale = Vector3.one * s;
            }
            if (iconGlow != null)
            {
                iconGlow.color = new Color(_initialIconGlowColor.r, _initialIconGlowColor.g, _initialIconGlowColor.b, alpha * 0.75f);
            }'''

replacement2 = '''            if (abilityPulseEffect != null)
            {
                abilityPulseEffect.color = new Color(_initialPulseColor.r, _initialPulseColor.g, _initialPulseColor.b, alpha);
                float s = Mathf.Lerp(0.98f, 1.05f, t);
                abilityPulseEffect.transform.localScale = _initialPulseScale * s;
            }
            if (iconGlow != null)
            {
                iconGlow.color = new Color(_initialIconGlowColor.r, _initialIconGlowColor.g, _initialIconGlowColor.b, alpha * 0.75f);
                float s = Mathf.Lerp(0.97f, 1.03f, t);
                iconGlow.transform.localScale = _initialIconGlowScale * s;
            }'''

target3 = '''    public void OnWidgetTapped()
    {
        AudioManager.Instance?.PlayButtonClick();
        TriggerPunchScale();

        if (!showFloatingTooltip) return;'''

replacement3 = '''    public void OnWidgetTapped()
    {
        AudioManager.Instance?.PlayButtonClick();
        TriggerPunchScale();

        if (abilityFeedbackEffect != null)
        {
            StartCoroutine(FlashFeedbackRoutine());
        }

        HighlightCompatibleObstacles();

        if (!showFloatingTooltip) return;'''

highlight_method = '''    /// <summary>
    /// Highlights matching obstacles on the board to clearly inform player where to use the active ability.
    /// </summary>
    public void HighlightCompatibleObstacles()
    {
        var gates = FindObjectsOfType<GateComponent>();
        if (gates == null || gates.Length == 0) return;

        foreach (var g in gates)
        {
            if (g == null || g.IsOpen) continue;
            bool isCompatible = false;
            if (_currentCreature == CreatureType.Turtle && g.gateType == GateType.BreakableRock) isCompatible = true;
            else if (_currentCreature == CreatureType.Octopus && g.gateType == GateType.InkedHazard) isCompatible = true;
            else if (_currentCreature == CreatureType.Jellyfish && g.gateType == GateType.ElectricGate) isCompatible = true;

            if (isCompatible)
            {
                g.ShowHint("Tap me to use ability!");
            }
        }
    }
'''

assert target1 in text, "target1 not found"
text = text.replace(target1, replacement1, 1)

assert target2 in text, "target2 not found"
text = text.replace(target2, replacement2, 1)

assert target3 in text, "target3 not found"
text = text.replace(target3, replacement3 + "\n\n" + highlight_method, 1)

with open('Assets/Script/CreatureAbilityHUD.cs', 'w', encoding='utf-8') as f:
    f.write(text)

print('All 3 updates successfully applied to CreatureAbilityHUD.cs!')
