using System;
using UnityEngine;

[Serializable]
public class PlanetStats
{
    public float hp;
    public float defense;
    public float shield;
    public float expRate;
    public float drain;
    public float hpRegeneration;

    public PlanetStats()
    {
        hp = 0f;
        defense = 0f;
        shield = 0f;
        expRate = 0f;
        drain = 0f;
        hpRegeneration = 0f;
    }

    public PlanetStats(float hp, float defense, float shield, float expRate, float drain, float hpRegeneration)
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
        hp = 0f;
        defense = 0f;
        shield = 0f;
        expRate = 0f;
        drain = 0f;
        hpRegeneration = 0f;
    }

    public int HpInt => Mathf.RoundToInt(hp);
    public int DefenseInt => Mathf.RoundToInt(defense);
    public int ShieldInt => Mathf.RoundToInt(shield);
    public int DrainInt => Mathf.RoundToInt(drain);
    public int HpRegenerationInt => Mathf.RoundToInt(hpRegeneration);
    public int ExpRateInt => Mathf.RoundToInt(expRate);

}
