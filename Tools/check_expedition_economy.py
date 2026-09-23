#!/usr/bin/env python3
"""Deterministic economy budget, not a Unity combat simulation.
Run from the repository root: python Tools/check_expedition_economy.py
Inputs are read from the authored runtime sources and catalogues.
"""
import json
import math
import re
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
CODE = ROOT / 'Assets/_Project/Code'
DEFS = ROOT / 'Assets/_Project/Resources/Definitions'

def number(pattern, text):
    match = re.search(pattern, text, re.S)
    if not match:
        raise AssertionError(f'Cannot locate authored input: {pattern}')
    return float(match.group(1))

def main():
    equipment = json.loads((DEFS / 'equipment.json').read_text())
    ships = json.loads((DEFS / 'ships.json').read_text())['ships']
    sloop = next(s for s in ships if s['id'] == 'starter_sloop')
    rat = next(s for s in ships if s['id'] == 'rat_sails')
    cannon = next(c for c in equipment['cannons'] if c['id'] == 'iron_6lb')
    shot = next(a for a in equipment['ammunition'] if a['id'] == 'standard')
    services = (CODE / 'Harbor/PrototypeHarborServices.cs').read_text()
    hull_rate = number(r'hullRepairSilverPerPoint\s*=\s*([\d.]+)f', services)
    subsystem_rate = number(r'subsystemRepairSilverPerPoint\s*=\s*([\d.]+)f', services)
    enemy_base = number(r'EnemyBaseHull\s*=\s*([\d.]+)f',
                        (CODE / 'Combat/EnemyCombatBalanceDirector.cs').read_text())
    ai = (CODE / 'Ship/EnemyShipController.cs').read_text()
    loot = (CODE / 'Combat/Loot/PrototypeShipwreckLoot.cs').read_text()
    reward_block = loot.split('int baseSilver = archetype switch')[1].split('};')[0]
    roles = [('Skirmisher', 'Skirmisher'), ('Marauder', None), ('Gunship', 'Gunship')]
    profiles = []
    for role, case in roles:
        marker = f'case EnemyShipArchetype.{case}:' if case else 'default:'
        section = ai.split(marker, 1)[1].split('break;', 1)[0]
        multiplier = number(r'SetRuntimeMaximumHealthMultiplier\(\s*([\d.]+)f', section)
        reward = int(number(r'(?:EnemyShipArchetype\.' + role + r'|_)\s*=>\s*(\d+)', reward_block)) if role == 'Marauder' else int(number(r'EnemyShipArchetype\.' + role + r'\s*=>\s*(\d+)', reward_block))
        profiles.append((role, enemy_base * multiplier, reward))
    # Three working muzzles on one side of the starter ship; no alternating-side shortcut.
    guns = min(3, sloop['startingCannons'] // 2)
    damage = cannon['damage'] * shot['damageMultiplier'] * shot['projectilesPerCannon']
    unit_cost = shot['silverPricePerBundle'] / shot['bundleSize']
    print('| Scenario | Enemy | Hull | Salvos | Ammo cost | Repair | Cargo | Net |')
    print('|---|---|---:|---:|---:|---:|---:|---:|')
    for label, accuracy, hull_loss, subsystem_loss in [
        ('Clean', 1.0, 0.10, 10), ('Baseline', 0.65, 0.25, 40), ('Costly', 0.40, 0.60, 120)
    ]:
        repair = math.ceil(sloop['maximumHealth'] * hull_loss * hull_rate + subsystem_loss * subsystem_rate)
        for role, hull, reward in profiles:
            salvos = math.ceil(hull / (guns * damage * accuracy))
            ammo = salvos * guns * unit_cost
            net = reward - ammo - repair
            assert reward <= sloop['cargoCapacity'], (role, 'uncollectable wreck')
            if label == 'Baseline':
                assert net >= 50, (role, 'baseline profit below 50 without bounty')
            print(f'| {label} | {role} | {hull:.0f} | {salvos} | {ammo:.1f} | {repair} | {reward} | {net:.1f} |')
    rewards = {role: value for role, _, value in profiles}
    assert rewards['Skirmisher'] + rewards['Marauder'] <= sloop['cargoCapacity']
    assert 2 * rewards['Gunship'] <= rat['cargoCapacity']
    # Worst regional single-wreck reward (west x1.5) must remain collectable by the starter.
    assert math.ceil(max(rewards.values()) * 1.5) <= sloop['cargoCapacity']
    for a in equipment['ammunition']:
        assert a['bundleSize'] > 0 and a['silverPricePerBundle'] >= 0
    print('\nPASS: baseline profit, two-wreck cargo budgets, regional single-wreck capacity and supply inputs.')
    print('Excludes quest rewards, recovered ammunition/material value, consumables, field repair and sinking.')
    print('Repair damage is an explicit scenario input; hit rate is an expectation, not projectile simulation.')

if __name__ == '__main__':
    main()
