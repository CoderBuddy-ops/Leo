using System;

namespace Leo.Core.Companion;

/// <summary>
/// 32-bit Mulberry32 deterministic PRNG.
/// Uses increment constant 1831565813 (0x6D2B79F5) and xor/imul operations matching verified HeyNixi generator.
/// </summary>
public class Mulberry32
{
    private uint _state;

    public Mulberry32(uint seed)
    {
        _state = seed;
    }

    public double NextDouble()
    {
        unchecked
        {
            _state = (uint)(_state + 1831565813u);
            uint n = (uint)((_state ^ (_state >> 15)) * (1u | _state));
            n = (uint)(n + (uint)((n ^ (n >> 7)) * (61u | n)) ^ n);
            return (double)(uint)(n ^ (n >> 14)) / 4294967296.0;
        }
    }
}
