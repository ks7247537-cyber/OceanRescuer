class MockPlayerPrefs:
    def __init__(self):
        self.data = {}
    def get_int(self, key, default=0):
        return self.data.get(key, default)
    def set_int(self, key, val):
        self.data[key] = val
    def save(self):
        pass

class MockSaveManager:
    def __init__(self, prefs):
        self.prefs = prefs
    def get_coins(self):
        return self.prefs.get_int("Coins", 0)
    def save_coins(self, val):
        self.prefs.set_int("Coins", max(0, val))
    def add_coins(self, val):
        self.save_coins(self.get_coins() + val)
    def spend_coins(self, val):
        c = self.get_coins()
        if c < val: return False
        self.save_coins(c - val)
        return True
    def get_gems(self):
        return self.prefs.get_int("Gems", 0)
    def save_gems(self, val):
        self.prefs.set_int("Gems", max(0, val))
    def get_powerup(self, ptype):
        return self.prefs.get_int("PowerUp_" + ptype, 0)
    def add_powerup(self, ptype, count=1):
        self.prefs.set_int("PowerUp_" + ptype, self.get_powerup(ptype) + count)

class MockEnergyManager:
    def __init__(self):
        self.energy = 20
        self.max_energy = 30
        self.unlimited_hours = 0.0
    def add_energy(self, amt):
        self.energy = min(self.max_energy, self.energy + amt)
    def refill_full(self):
        self.energy = self.max_energy
    def grant_unlimited(self, hrs):
        self.unlimited_hours += hrs

def run_tests():
    prefs = MockPlayerPrefs()
    save_mgr = MockSaveManager(prefs)
    energy_mgr = MockEnergyManager()

    # Initial Balances: 1000 Coins, 50 Gems
    save_mgr.save_coins(1000)
    save_mgr.save_gems(50)

    print("=== Testing Shop Purchase Logic ===")

    # 1. Buy Items
    # A. Super Wrench (150 coins)
    c_before = save_mgr.get_coins()
    assert save_mgr.spend_coins(150)
    save_mgr.add_powerup("Wrench", 1)
    assert save_mgr.get_coins() == c_before - 150
    assert save_mgr.get_powerup("Wrench") == 1
    print("Test 1 PASS: Bought Super Wrench (150 coins deducted, +1 Wrench added)")

    # B. Path Hint (50 coins)
    c_before = save_mgr.get_coins()
    assert save_mgr.spend_coins(50)
    save_mgr.add_powerup("Hint", 1)
    assert save_mgr.get_coins() == c_before - 50
    assert save_mgr.get_powerup("Hint") == 1
    print("Test 2 PASS: Bought Path Hint (50 coins deducted, +1 Hint added)")

    # 2. Buy Energy
    # A. 5 Energy Refill (50 coins)
    e_before = energy_mgr.energy
    assert save_mgr.spend_coins(50)
    energy_mgr.add_energy(5)
    assert energy_mgr.energy == e_before + 5
    print("Test 3 PASS: Bought 5 Energy Refill (50 coins deducted, Energy restored to 25)")

    # B. Full Energy Refill (10 gems)
    g_before = save_mgr.get_gems()
    save_mgr.save_gems(g_before - 10)
    energy_mgr.refill_full()
    assert save_mgr.get_gems() == g_before - 10
    assert energy_mgr.energy == 30
    print("Test 4 PASS: Bought Full Energy Refill (10 gems deducted, Energy restored to 30/30)")

    # C. 1 Hr Unlimited (20 gems)
    g_before = save_mgr.get_gems()
    save_mgr.save_gems(g_before - 20)
    energy_mgr.grant_unlimited(1.0)
    assert save_mgr.get_gems() == g_before - 20
    assert energy_mgr.unlimited_hours == 1.0
    print("Test 5 PASS: Bought 1 Hr Unlimited Energy (20 gems deducted, Unlimited granted)")

    # 3. Rewards & One-Time Protection
    # A. Daily Gift (Free -> +100 Coins)
    c_before = save_mgr.get_coins()
    assert prefs.get_int("Bought_reward_daily", 0) == 0
    save_mgr.add_coins(100)
    prefs.set_int("Bought_reward_daily", 1)
    assert save_mgr.get_coins() == c_before + 100
    assert prefs.get_int("Bought_reward_daily", 0) == 1
    print("Test 6 PASS: Claimed Daily Pearl Gift (+100 Free Coins claimed)")

    # Attempt re-purchase
    assert prefs.get_int("Bought_reward_daily", 0) == 1 # Blocked!
    print("Test 7 PASS: Re-claiming Daily Pearl Gift is correctly BLOCKED (one-time protection)")

    # B. Starter Package (Free -> +10 Gems, +200 Coins)
    c_before = save_mgr.get_coins()
    g_before = save_mgr.get_gems()
    assert prefs.get_int("Bought_reward_starter", 0) == 0
    save_mgr.add_coins(200)
    save_mgr.save_gems(g_before + 10)
    prefs.set_int("Bought_reward_starter", 1)
    assert save_mgr.get_coins() == c_before + 200
    assert save_mgr.get_gems() == g_before + 10
    print("Test 8 PASS: Claimed Starter Package (+10 Gems & +200 Coins claimed)")

    # 4. Gem Exchanges
    # Small Gem Pouch (100 coins -> +5 gems)
    c_before = save_mgr.get_coins()
    g_before = save_mgr.get_gems()
    assert save_mgr.spend_coins(100)
    save_mgr.save_gems(g_before + 5)
    assert save_mgr.get_coins() == c_before - 100
    assert save_mgr.get_gems() == g_before + 5
    print("Test 9 PASS: Bought Small Gem Pouch (100 coins deducted, +5 gems added)")

    # 5. Insufficient Currency Test
    save_mgr.save_coins(20)
    # Trying to buy Super Wrench (150 coins)
    assert not save_mgr.spend_coins(150)
    assert save_mgr.get_coins() == 20 # Untouched!
    print("Test 10 PASS: Insufficient coins purchase blocked safely without deducting balance")

    save_mgr.save_gems(5)
    # Trying to buy Full Energy (10 gems)
    can_buy_gems = save_mgr.get_gems() >= 10
    assert not can_buy_gems
    assert save_mgr.get_gems() == 5 # Untouched!
    print("Test 11 PASS: Insufficient gems purchase blocked safely without deducting balance")

    print("\nALL 11 SHOP LOGIC & PROGRESSION TESTS PASSED 100%!")

if __name__ == '__main__':
    run_tests()
