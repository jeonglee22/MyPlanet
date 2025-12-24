using System;
using UnityEngine;

[Serializable]
public class PlanetStats
{
    public int hp;
    public int defense;
    public int shield;
    public int expRate;
    public int drain;
    public int hpRegeneration;

    public PlanetStats()
    {
        hp = 0;
        defense = 0;
        shield = 0;
        expRate = 0;
        drain = 0;
        hpRegeneration = 0;
    }

    public PlanetStats(int hp, int defense, int shield, int expRate, int drain, int hpRegeneration)
    {
        this.hp = hp;
        this.defense = defense;
        this.shield = shield;
        this.expRate = expRate;
        this.drain = drain;
        this.hpRegeneration = hpRegeneration;
    }

    public static PlanetStats operator +(PlanetStats a, PlanetStats b)
    {
        return new PlanetStats
        {
            hp = a.hp + b.hp,
            defense = a.defense + b.defense,
            shield = a.shield + b.shield,
            expRate = a.expRate + b.expRate,
            drain = a.drain + b.drain,
            hpRegeneration = a.hpRegeneration + b.hpRegeneration
        };
    }

    public void Clear()
    {
        hp = 0;
        defense = 0;
        shield = 0;
        expRate = 0;
        drain = 0;
        hpRegeneration = 0;
    }
}
