using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using PokemonPRNG.SFMT;
using PokemonPRNG.SFMT.SIMD;
using PokemonPRNG.MT;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using System.Runtime.CompilerServices;
using Microsoft.Diagnostics.Tracing.StackSources;
using static System.Buffers.Binary.BinaryPrimitives;
using System.Numerics;
using System.Runtime.InteropServices;

var switcher = new BenchmarkSwitcher(new[]
{
    typeof(SFMTBenchmark),
    typeof(MTBenchmark),
    typeof(SIMDLCGBenchmark),
    typeof(SIMDMTBenchmark),
    typeof(SIMDMTIVsBenchmark),
    typeof(EndianBenchmark),
});

args = new string[] { "0" };
switcher.Run(args);


public class BenchmarkConfig : ManualConfig
{
    public BenchmarkConfig()
    {
        AddExporter(MarkdownExporter.GitHub); // ベンチマーク結果を書く時に出力させとくとベンリ
        AddDiagnoser(MemoryDiagnoser.Default);

        // ShortRunを使うとサクッと終わらせられる、デフォルトだと本気で長いので短めにしとく。
        // ShortRunは LaunchCount=1  TargetCount=3 WarmupCount = 3 のショートカット
        //AddJob(Job.ShortRun);
    }
}

[Config(typeof(BenchmarkConfig))]
public class SFMTBenchmark
{
    private readonly uint initialSeed = 0xBEEFFACE;

    private SFMT? sfmt;
    private CachedSFMT? cached;
    private SIMDSFMT? simd;
    private CachedSIMDSFMT? simd_cached;

    [IterationSetup]
    public void Setup()
    {
        sfmt = new(initialSeed);
        cached = new(initialSeed, 3);
        simd = new(initialSeed);
        simd_cached = new(initialSeed, 3);
    }

    public IEnumerable<object[]> Loops()
    {
        yield return new object[] { 10000000, 1 };
        yield return new object[] { 100000, 100 };
        yield return new object[] { 20000, 500 };
    }

    [Benchmark(Baseline = true)]
    [ArgumentsSource(nameof(Loops))]
    public long SFMT(int mainLoop, int innerLoop)
    {
        var sum = 0u;
        for (int i = 0; i < mainLoop; i++, sfmt.Advance())
        {
            var temp = sfmt!.Clone();
            for (int k = 0; k < innerLoop; k++)
                sum += temp.GetRand32();
        }

        return sum;
    }

    [Benchmark()]
    [ArgumentsSource(nameof(Loops))]
    public long CachedSFMT(int mainLoop, int innerLoop)
    {
        var sum = 0u;
        for (int i = 0; i < mainLoop; i++, cached!.MoveNext())
        {
            for (int k = 0; k < innerLoop; k++)
                sum += cached!.GetRand32();
        }

        return sum;
    }

    [Benchmark]
    [ArgumentsSource(nameof(Loops))]
    public long SIMDSFMT(int mainLoop, int innerLoop)
    {
        var sum = 0u;
        for (int i = 0; i < mainLoop; i++, simd.Advance())
        {
            var temp = simd!.Clone();
            for (int k = 0; k < innerLoop; k++)
                sum += temp.GetRand32();
        }

        return sum;
    }

    [Benchmark]
    [ArgumentsSource(nameof(Loops))]
    public long CachedSIMDSFMT(int mainLoop, int innerLoop)
    {
        var sum = 0u;
        for (int i = 0; i < mainLoop; i++, simd_cached!.MoveNext())
        {
            for (int k = 0; k < innerLoop; k++)
                sum += simd_cached!.GetRand32();
        }

        return sum;
    }
}

[Config(typeof(BenchmarkConfig))]
public class MTBenchmark
{
    private readonly uint initialSeed = 0xBEEFFACE;

    private MT? mt;
    private CachedMT? cached;

    [IterationSetup]
    public void Setup()
    {
        mt = new MT(initialSeed);
        cached = new CachedMT(initialSeed, 3);
    }

    public IEnumerable<object[]> Loops()
    {
        yield return new object[] { 1000000, 100 };
        yield return new object[] { 20000, 5000 };
    }

    [Benchmark(Baseline = true)]
    [ArgumentsSource(nameof(Loops))]
    public long MT(int mainLoop, int innerLoop)
    {
        var sum = 0u;
        for (int i = 0; i < mainLoop; i++, mt.Advance())
        {
            var temp = mt!.Clone();
            for (int k = 0; k < innerLoop; k++)
                sum += temp.GetRand();
        }

        return sum;
    }

    [Benchmark]
    [ArgumentsSource(nameof(Loops))]
    public long Cached(int mainLoop, int innerLoop)
    {
        var sum = 0u;
        for (int i = 0; i < mainLoop; i++, cached!.MoveNext())
        {
            for (int k = 0; k < innerLoop; k++)
                sum += cached!.GetRand();
        }

        return sum;
    }
}


[Config(typeof(BenchmarkConfig))]
public class SIMDLCGBenchmark
{
    [Benchmark(Baseline = true)]
    public void Normal()
    {
        var seed0 = 0x0u;
        var seed1 = 0x12345678u;
        var seed2 = 0xF0F0F0F0u;
        var seed3 = 0xBEEFFACEu;
        for (int i = 0; i < 50_0000; i++)
        {
            seed0 = seed0 * 0x41c64e6du + 0x6073u;
            seed1 = seed1 * 0x41c64e6du + 0x6073u;
            seed2 = seed2 * 0x41c64e6du + 0x6073u;
            seed3 = seed3 * 0x41c64e6du + 0x6073u;
        }
    }

    [Benchmark()]
    public void SIMD()
    {
        var seeds = Vector128.Create(0x0u, 0x12345678u, 0xF0F0F0F0u, 0xBEEFFACEu);
        var mul = Vector128.Create(0x41c64e6du);
        var add = Vector128.Create(0x6073u);

        for (int i = 0; i < 50_0000; i++)
        {
            seeds = Avx2.Add(Avx2.MultiplyLow(seeds, mul), add);
        }
    }
}

[Config(typeof(BenchmarkConfig))]
public class SIMDMTBenchmark
{
    [Benchmark(Baseline = true)]
    public void Normal()
    {
        var mt0 = new MT(0x1u);
        var mt1 = new MT(0x12345678u);
        var mt2 = new MT(0xF0F0F0F0u);
        var mt3 = new MT(0x77777777u);

        var sum = 0u;
        for (int i = 0; i < 1000000; i++)
        {
            sum += mt0.GetRand();
            sum += mt1.GetRand();
            sum += mt2.GetRand();
            sum += mt3.GetRand();
        }
    }

    [Benchmark()]
    public void SIMD()
    {
        var seeds = Vector128.Create(0x1u, 0x12345678u, 0xF0F0F0F0u, 0x77777777u);
        var mt = new MultipleMT4(seeds);

        var sum = 0u;
        for (int i = 0; i < 1000000; i++)
        {
            var rand = mt.GetRand();
            sum += rand[0];
            sum += rand[1];
            sum += rand[2];
            sum += rand[3];
        }
    }

    //[Benchmark()]
    public void SIMD2()
    {
        var seeds = Vector256.Create(0x1u, 0x12345678u, 0xF0F0F0F0u, 0x77777777u, 0x1u, 0x12345678u, 0xF0F0F0F0u, 0x77777777u);
        var mt = new MultipleMT8(seeds);

        var sum = 0u;
        for (int i = 0; i < 1000000; i++)
        {
            var rand = mt.GetRand();
            sum += rand[0];
            sum += rand[1];
            sum += rand[2];
            sum += rand[3];
            sum += rand[4];
            sum += rand[5];
            sum += rand[6];
            sum += rand[7];
        }
    }
}

[Config(typeof(BenchmarkConfig))]
public class SIMDMTIVsBenchmark
{
    const uint MAX = 0x20_0000;

    //[Benchmark(Baseline = true)]
    public void Normal()
    {
        var cnt = 0;
        var upper = 0x1u;
        for (uint seed = 0x0; seed < MAX; seed++)
        {
            var ivs = MT__.GetBWIVsCode(upper | seed);
            if (ivs == 0x3FFFFFFF) cnt++;

        }
    }

    [Benchmark(Baseline = true)]
    public void SIMD4()
    {
        var cnt = 0;
        var upper = 0x1u;
        for (uint seed = 0x0; seed < MAX; seed += 4)
        {
            var head = upper | seed;
            var ivs = MT128.GetBWIVsCode(Vector128.Create(head, head + 1, head + 2, head + 3));

            for (int k = 0; k < 4; k++)
                if (ivs[k] == 0x3FFFFFFF) cnt++;
        }
    }

    [Benchmark()]
    public void SIMD4_2()
    {
        var cnt = 0;
        var upper = 0x1u;
        for (uint seed = 0x0; seed < MAX; seed += 4)
        {
            var head = upper | seed;
            var ivs = MT128.GetBWIVsCode2(Vector128.Create(head, head + 1, head + 2, head + 3));

            for (int k = 0; k < 4; k++)
                if (ivs[k] == 0x3FFFFFFF) cnt++;
        }
    }

    //[Benchmark()]
    public void SIMD8()
    {
        var cnt = 0;
        var upper = 0x1u;
        for (uint seed = 0x0; seed < MAX; seed += 8)
        {
            var head = upper | seed;
            var ivs = MT256.GetBWIVsCode(Vector256.Create(head, head + 1, head + 2, head + 3, head + 4, head + 5, head + 6, head + 7));

            for (int k = 0; k < 8; k++)
                if (ivs[k] == 0x3FFFFFFF) cnt++;
        }
    }
}

[Config(typeof(BenchmarkConfig))]
public class EndianBenchmark
{
    const uint MAX = 0x8_0000;

    private InitialSeedGenerator__ gen1;
    private InitialSeedGenerator2__ gen2;

    [IterationSetup]
    public void Setup()
    {
        gen1 = new InitialSeedGenerator__(
            new uint[] { 0, 0x21, 0x47, 0x47, 0x26, 0xf4 },
            new uint[] { 0x2215f10, 0x221600C, 0x221600C, 0x2216058, 0x2216058 },
            0x60,
            6,
            0xC7A
        );
        gen2 = new InitialSeedGenerator2__(
            new uint[] { 0, 0x21, 0x47, 0x47, 0x26, 0xf4 },
            new uint[] { 0x2215f10, 0x221600C, 0x221600C, 0x2216058, 0x2216058 },
            0x60,
            6,
            0xC7A
        );
    }


    [Benchmark(Baseline = true)]
    public void Base()
    {
        var generator = new InitialSeedGeneratorBase(
            new uint[] { 0, 0x21, 0x47, 0x47, 0x26, 0xf4 },
            new uint[] { 0x2215f10, 0x221600C, 0x221600C, 0x2216058, 0x2216058 },
            0x60,
            6,
            0xC7A
        );

        var sum = 0u;
        for (int i = 0; i < MAX; i++)
            sum += generator.GenerateMTSeed(0xDEADBEEF, 0xBEEFFACE);
    }

    [Benchmark]
    public void A()
    {
        var sum = 0ul;
        for (uint i = 0; i < 30; i++)
            sum += gen1.GenerateMTSeed();
    }

    [Benchmark]
    public void B()
    {
        var sum = 0ul;
        for (uint i = 0; i < 30; i++)
            sum += gen2.GenerateMTSeed();
    }

    //[Benchmark]
    public void C()
    {
        var generator = new InitialSeedGenerator3(
            new uint[] { 0, 0x21, 0x47, 0x47, 0x26, 0xf4 },
            new uint[] { 0x2215f10, 0x221600C, 0x221600C, 0x2216058, 0x2216058 },
            0x60,
            6,
            0xC7A
        );

        var sum = 0u;
        for (int i = 0; i < MAX; i++)
            sum += generator.GenerateMTSeed(0xDEADBEEF, 0xBEEFFACE);
    }
}


public class MT__
{
    public static uint GetBWIVsCode(uint seed)
    {
        var table = new uint[403];
        table[0] = seed;
        for (uint i = 1; i < 403; i++)
            table[i] = 0x6C078965u * (table[i - 1] ^ (table[i - 1] >> 30)) + i;

        uint ivsCode = 0;
        for (var i = 0; i < 6; i++)
        {
            var temp = (table[i] & 0x80000000) | (table[i + 1] & 0x7FFFFFFF);
            var val = table[i + 397] ^ (temp >> 1);
            if ((temp & 1) == 1) val ^= 0x9908b0df;

            val ^= (val >> 11);
            val ^= (val << 7) & 0x9d2c5680;
            val ^= (val << 15) & 0xefc60000;
            val ^= (val >> 18);

            val >>= 27;

            ivsCode |= val << (5 * i);
        }

        return ivsCode;
    }
}

class MT128
{
    private static readonly Vector128<uint> ONE = Vector128.Create(1u);

    private static readonly Vector128<uint> MATRIX_A = Vector128.Create(0x9908b0dfu);
    private static readonly Vector128<uint> UPPER_MASK = Vector128.Create(0x80000000u);
    private static readonly Vector128<uint> LOWER_MASK = Vector128.Create(0x7fffffffu);

    private static readonly Vector128<uint> MATRIX_TEMPER_1 = Vector128.Create(0x9d2c5680u);
    private static readonly Vector128<uint> MATRIX_TEMPER_2 = Vector128.Create(0xefc60000u);

    public static Vector128<uint> GetBWIVsCode(in Vector128<uint> initialSeeds)
    {
        var _stateVector = new Vector128<uint>[403];

        _stateVector[0] = initialSeeds;
        for (uint i = 1; i < _stateVector.Length; i++)
            _stateVector[i] = 0x6C078965u * (_stateVector[i - 1] ^ (Vector128.ShiftRightLogical(_stateVector[i - 1], 30))) + Vector128.Create(i);

        var ivsCodes = Vector128<uint>.Zero;
        for (var k = 0; k < 6; k++)
        {
            var temp = (_stateVector[k] & UPPER_MASK) | (_stateVector[k + 1] & LOWER_MASK);
            var val = _stateVector[k + 397] ^ Vector128.ShiftRightLogical(temp, 1)
                ^ (MATRIX_A * (temp & ONE));

            val ^= Vector128.ShiftRightLogical(val, 11);
            val ^= Vector128.ShiftLeft(val, 7) & MATRIX_TEMPER_1;
            val ^= Vector128.ShiftLeft(val, 15) & MATRIX_TEMPER_2;
            val ^= Vector128.ShiftRightLogical(val, 18);

            ivsCodes |= Vector128.ShiftLeft(Vector128.ShiftRightLogical(val, 27), (5 * k));
        }

        return ivsCodes;
    }

    public static Vector128<uint> GetBWIVsCode2(in Vector128<uint> initialSeeds)
    {
        var _stateVector = new Vector128<uint>[403];

        _stateVector[0] = initialSeeds;
        for (uint i = 1; i < _stateVector.Length; i++)
            _stateVector[i] = 0x6C078965u * (_stateVector[i - 1] ^ (Vector128.ShiftRightLogical(_stateVector[i - 1], 30))) + Vector128.Create(i);

        var ivsCodes = Vector128<uint>.Zero;
        for (var k = 0; k < 6; k++)
        {
            var temp = (_stateVector[k] & UPPER_MASK) | (_stateVector[k + 1] & LOWER_MASK);
            var val = _stateVector[k + _stateVector.Length - 7] ^ Vector128.ShiftRightLogical(temp, 1)
                ^ (MATRIX_A * (temp & ONE));

            val ^= Vector128.ShiftRightLogical(val, 11);
            val ^= Vector128.ShiftLeft(val, 7) & MATRIX_TEMPER_1;
            val ^= Vector128.ShiftLeft(val, 15) & MATRIX_TEMPER_2;
            val ^= Vector128.ShiftRightLogical(val, 18);

            ivsCodes |= Vector128.ShiftLeft(Vector128.ShiftRightLogical(val, 27), (5 * k));
        }

        return ivsCodes;
    }

    public static Vector128<uint> GetBWIVsCode(uint initialSeed)
    {
        var _stateVector = new Vector128<uint>[403];

        _stateVector[0] = Vector128.Create(initialSeed, initialSeed + 1, initialSeed + 2, initialSeed + 3);
        for (uint i = 1; i < _stateVector.Length; i++)
            _stateVector[i] = 0x6C078965u * (_stateVector[i - 1] ^ (Vector128.ShiftRightLogical(_stateVector[i - 1], 30))) + Vector128.Create(i);

        var ivsCodes = Vector128<uint>.Zero;
        for (var k = 0; k < 6; k++)
        {
            var temp = (_stateVector[k] & UPPER_MASK) | (_stateVector[k + 1] & LOWER_MASK);
            var val = _stateVector[k + 397] ^ Vector128.ShiftRightLogical(temp, 1)
                ^ (MATRIX_A * (temp & ONE));

            val ^= Vector128.ShiftRightLogical(val, 11);
            val ^= Vector128.ShiftLeft(val, 7) & MATRIX_TEMPER_1;
            val ^= Vector128.ShiftLeft(val, 15) & MATRIX_TEMPER_2;
            val ^= Vector128.ShiftRightLogical(val, 18);

            ivsCodes |= Vector128.ShiftLeft(Vector128.ShiftRightLogical(val, 27), (5 * k));
        }

        return ivsCodes;
    }
}

class MT256
{
    private static readonly Vector256<uint> ONE = Vector256.Create(1u);

    private static readonly Vector256<uint> MATRIX_A = Vector256.Create(0x9908b0dfu);
    private static readonly Vector256<uint> UPPER_MASK = Vector256.Create(0x80000000u);
    private static readonly Vector256<uint> LOWER_MASK = Vector256.Create(0x7fffffffu);

    private static readonly Vector256<uint> MATRIX_TEMPER_1 = Vector256.Create(0x9d2c5680u);
    private static readonly Vector256<uint> MATRIX_TEMPER_2 = Vector256.Create(0xefc60000u);

    public static Vector256<uint> GetBWIVsCode(in Vector256<uint> initialSeeds)
    {
        var _stateVector = new Vector256<uint>[403];

        _stateVector[0] = initialSeeds;
        for (uint i = 1; i < _stateVector.Length; i++)
            _stateVector[i] = 0x6C078965u * (_stateVector[i - 1] ^ (Vector256.ShiftRightLogical(_stateVector[i - 1], 30))) + Vector256.Create(i);

        var ivsCodes = Vector256<uint>.Zero;
        for (var k = 0; k < 6; k++)
        {
            var temp = (_stateVector[k] & UPPER_MASK) | (_stateVector[k + 1] & LOWER_MASK);
            var val = _stateVector[k + 397] ^ Vector256.ShiftRightLogical(temp, 1)
                ^ (MATRIX_A * (temp & ONE));

            val ^= Vector256.ShiftRightLogical(val, 11);
            val ^= Vector256.ShiftLeft(val, 7) & MATRIX_TEMPER_1;
            val ^= Vector256.ShiftLeft(val, 15) & MATRIX_TEMPER_2;
            val ^= Vector256.ShiftRightLogical(val, 18);

            ivsCodes |= Vector256.ShiftLeft(Vector256.ShiftRightLogical(val, 27), (5 * k));
        }

        return ivsCodes;
    }

}

public class MultipleMT4
{
    protected const int N = 624;
    protected const int M = 397;
    private static readonly Vector128<uint> ONE = Vector128.Create(1u);

    private static readonly Vector128<uint> MATRIX_A = Vector128.Create(0x9908b0dfu);
    private static readonly Vector128<uint> UPPER_MASK = Vector128.Create(0x80000000u);
    private static readonly Vector128<uint> LOWER_MASK = Vector128.Create(0x7fffffffu);

    private static readonly Vector128<uint> MATRIX_TEMPER_1 = Vector128.Create(0x9d2c5680u);
    private static readonly Vector128<uint> MATRIX_TEMPER_2 = Vector128.Create(0xefc60000u);

    protected readonly Vector128<uint>[] _stateVector;

    private int _randIndex;
    public Vector128<uint> GetRand()
    {
        if (_randIndex >= N)
        {
            Update();
            _randIndex = 0;
        }

        return Temper(_stateVector[_randIndex++]);
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector128<uint> Temper(Vector128<uint> y)
    {
        y ^= Vector128.ShiftRightLogical(y, 11);
        y ^= Vector128.ShiftLeft(y, 7) & MATRIX_TEMPER_1;
        y ^= Vector128.ShiftLeft(y, 15) & MATRIX_TEMPER_2;
        y ^= Vector128.ShiftRightLogical(y, 18);
        return y;
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Update()
    {
        for (var k = 0; k < N - M; k++)
        {
            var temp = (_stateVector[k] & UPPER_MASK) | (_stateVector[k + 1] & LOWER_MASK);
            _stateVector[k] =
                _stateVector[k + M]
                ^ Vector128.ShiftRightLogical(temp, 1)
                ^ (MATRIX_A * (temp & ONE));
        }
        for (var k = N - M; k < N - 1; k++)
        {
            var temp = (_stateVector[k] & UPPER_MASK) | (_stateVector[k + 1] & LOWER_MASK);
            _stateVector[k] =
                _stateVector[k + (M - N)]
                ^ Vector128.ShiftRightLogical(temp, 1)
                ^ (MATRIX_A * (temp & ONE));
        }
        {
            var temp = (_stateVector[N - 1] & UPPER_MASK) | (_stateVector[0] & LOWER_MASK);
            _stateVector[N - 1] =
                _stateVector[M - 1]
                ^ Vector128.ShiftRightLogical(temp, 1)
                ^ (MATRIX_A * (temp & ONE));
        }
    }

    public MultipleMT4(in Vector128<uint> initialSeeds)
    {
        _stateVector = new Vector128<uint>[N];

        _stateVector[0] = initialSeeds;
        for (uint i = 1; i < _stateVector.Length; i++)
            _stateVector[i] = 0x6C078965u * (_stateVector[i - 1] ^ (Vector128.ShiftRightLogical(_stateVector[i - 1], 30))) + Vector128.Create(i);

        _randIndex = N;
    }

}

public class MultipleMT8
{
    protected const int N = 624;
    protected const int M = 397;
    private static readonly Vector256<uint> ONE = Vector256.Create(1u);

    private static readonly Vector256<uint> MATRIX_A = Vector256.Create(0x9908b0dfu);
    private static readonly Vector256<uint> UPPER_MASK = Vector256.Create(0x80000000u);
    private static readonly Vector256<uint> LOWER_MASK = Vector256.Create(0x7fffffffu);

    private static readonly Vector256<uint> MATRIX_TEMPER_1 = Vector256.Create(0x9d2c5680u);
    private static readonly Vector256<uint> MATRIX_TEMPER_2 = Vector256.Create(0xefc60000u);

    protected readonly Vector256<uint>[] _stateVector;

    private int _randIndex;
    public Vector256<uint> GetRand()
    {
        if (_randIndex >= N)
        {
            Update();
            _randIndex = 0;
        }

        return Temper(_stateVector[_randIndex++]);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Vector256<uint> Temper(Vector256<uint> y)
    {
        y ^= Vector256.ShiftRightLogical(y, 11);
        y ^= Vector256.ShiftLeft(y, 7) & MATRIX_TEMPER_1;
        y ^= Vector256.ShiftLeft(y, 15) & MATRIX_TEMPER_2;
        y ^= Vector256.ShiftRightLogical(y, 18);
        return y;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void Update()
    {
        for (var k = 0; k < N - M; k++)
        {
            var temp = (_stateVector[k] & UPPER_MASK) | (_stateVector[k + 1] & LOWER_MASK);
            _stateVector[k] =
                _stateVector[k + M]
                ^ Vector256.ShiftRightLogical(temp, 1)
                ^ (MATRIX_A * (temp & ONE));
        }
        for (var k = N - M; k < N - 1; k++)
        {
            var temp = (_stateVector[k] & UPPER_MASK) | (_stateVector[k + 1] & LOWER_MASK);
            _stateVector[k] =
                _stateVector[k + (M - N)]
                ^ Vector256.ShiftRightLogical(temp, 1)
                ^ (MATRIX_A * (temp & ONE));
        }
        {
            var temp = (_stateVector[N - 1] & UPPER_MASK) | (_stateVector[0] & LOWER_MASK);
            _stateVector[N - 1] =
                _stateVector[M - 1]
                ^ Vector256.ShiftRightLogical(temp, 1)
                ^ (MATRIX_A * (temp & ONE));
        }
    }

    public MultipleMT8(in Vector256<uint> initialSeeds)
    {
        _stateVector = new Vector256<uint>[N];

        _stateVector[0] = initialSeeds;
        for (uint i = 1; i < _stateVector.Length; i++)
            _stateVector[i] = 0x6C078965u * (_stateVector[i - 1] ^ (Vector256.ShiftRightLogical(_stateVector[i - 1], 30))) + Vector256.Create(i);

        _randIndex = N;
    }

}


class InitialSeedGenerator
{
    const uint H0 = 0x67452301;
    const uint H1 = 0xEFCDAB89;
    const uint H2 = 0x98BADCFE;
    const uint H3 = 0x10325476;
    const uint H4 = 0xC3D2E1F0;

    const int N = 80;

    private readonly uint[] W = new uint[N];
    public InitialSeedGenerator(uint[] mac, uint[] nazo, uint v, uint frame, uint t0)
    {
        W[0] = ReverseEndianness(nazo[0]);
        W[1] = ReverseEndianness(nazo[1]);
        W[2] = ReverseEndianness(nazo[2]);
        W[3] = ReverseEndianness(nazo[3]);
        W[4] = ReverseEndianness(nazo[4]);

        W[5] = ReverseEndianness((v << 16) | t0);
        W[6] = (mac[4] << 8) | mac[5];
        W[7] = ReverseEndianness(0x6000000 ^ frame ^ (mac[3] << 24) | (mac[2] << 16) | (mac[1] << 8) | (mac[0]));

        W[10] = 0x00000000;
        W[11] = 0x00000000;
        W[12] = 0xFF2F0000;
        W[13] = 0x80000000;
        W[14] = 0x00000000;
        W[15] = 0x000001A0;
    }

    public ulong GenerateMTSeed(uint dateCode, uint timeCode)
    {
        W[8] = dateCode;
        W[9] = timeCode;

        for (int t = 16; t < W.Length; t++)
        {
            var w = W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16];
            W[t] = BitOperations.RotateLeft(w, 1);
        }

        var A = H0;
        var B = H1;
        var C = H2;
        var D = H3;
        var E = H4;

        foreach (var w in W.AsSpan().Slice(0, 20))
        {
            var temp = BitOperations.RotateLeft(A, 5) + ((B & C) | ((~B) & D)) + E + w + 0x5A827999;
            E = D;
            D = C;
            C = BitOperations.RotateRight(B, 2);
            B = A;
            A = temp;
        }
        foreach (var w in W.AsSpan().Slice(20, 20))
        {
            var temp = BitOperations.RotateLeft(A, 5) + (B ^ C ^ D) + E + w + 0x6ED9EBA1;
            E = D;
            D = C;
            C = BitOperations.RotateRight(B, 2);
            B = A;
            A = temp;
        }
        foreach (var w in W.AsSpan().Slice(40, 20))
        {
            var temp = BitOperations.RotateLeft(A, 5) + ((B & C) | (B & D) | (C & D)) + E + w + 0x8F1BBCDC;
            E = D;
            D = C;
            C = BitOperations.RotateRight(B, 2);
            B = A;
            A = temp;
        }
        foreach (var w in W.AsSpan().Slice(60, 20))
        {
            var temp = BitOperations.RotateLeft(A, 5) + (B ^ C ^ D) + E + w + 0xCA62C1D6;
            E = D;
            D = C;
            C = BitOperations.RotateRight(B, 2);
            B = A;
            A = temp;
        }

        return ((ulong)ReverseEndianness(H1 + B) << 32) | ReverseEndianness(H0 + A);
    }

}

class InitialSeedGenerator2
{
    private readonly Vector256<uint> H0 = Vector256.Create(0x67452301u);
    private readonly Vector256<uint> H1 = Vector256.Create(0xEFCDAB89);
    private readonly Vector256<uint> H2 = Vector256.Create(0x98BADCFE);
    private readonly Vector256<uint> H3 = Vector256.Create(0x10325476u);
    private readonly Vector256<uint> H4 = Vector256.Create(0xC3D2E1F0);

    private readonly Vector256<uint> C0 = Vector256.Create(0x5A827999u);
    private readonly Vector256<uint> C1 = Vector256.Create(0x6ED9EBA1u);
    private readonly Vector256<uint> C2 = Vector256.Create(0x8F1BBCDCu);
    private readonly Vector256<uint> C3 = Vector256.Create(0xCA62C1D6u);

    private readonly Vector256<uint> h8 = Vector256.Create(0u, 1, 2, 3, 4, 5, 6, 7);

    const int N = 80;

    private readonly Vector256<uint>[] W = new Vector256<uint>[N];
    public InitialSeedGenerator2(uint[] mac, uint[] nazo, uint v, uint frame, uint t0)
    {
        W[0] = Vector256.Create(ReverseEndianness(nazo[0]));
        W[1] = Vector256.Create(ReverseEndianness(nazo[1]));
        W[2] = Vector256.Create(ReverseEndianness(nazo[2]));
        W[3] = Vector256.Create(ReverseEndianness(nazo[3]));
        W[4] = Vector256.Create(ReverseEndianness(nazo[4]));

        W[5] = Vector256.Create(ReverseEndianness((v << 16) | t0));
        W[6] = Vector256.Create((mac[4] << 8) | mac[5]);
        W[7] = Vector256.Create(ReverseEndianness(0x6000000 ^ frame ^ (mac[3] << 24) | (mac[2] << 16) | (mac[1] << 8) | (mac[0])));

        W[10] = Vector256<uint>.Zero;
        W[11] = Vector256<uint>.Zero;
        W[12] = Vector256.Create(0xFF2F0000);
        W[13] = Vector256.Create(0x80000000);
        W[14] = Vector256<uint>.Zero;
        W[15] = Vector256.Create(0x000001A0u);
    }

    public ulong GenerateMTSeed(uint dateCode, uint timeCode)
    {
        W[8] = Vector256.Create(dateCode);
        W[9] = Vector256.Create(timeCode) | h8;

        for (int t = 16; t < W.Length; t++)
        {
            var w = W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16];
            W[t] = Vector256.ShiftLeft(w, 1) ^ Vector256.ShiftRightLogical(w, 31);
        }

        var (A, B, C, D, E) = (H0, H1, H2, H3, H4);

        foreach (var w in W.AsSpan().Slice(0, 20))
        {
            (A, B, C, D, E) =
                ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + ((B & C) | ((~B) & D)) + E + w + C0,
                A,
                (Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2)),
                C,
                D);
        }

        foreach (var w in W.AsSpan().Slice(20, 20))
        {
            (A, B, C, D, E) =
                ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + (B ^ C ^ D) + E + w + C1,
                A,
                Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2),
                C,
                D);
        }
        foreach (var w in W.AsSpan().Slice(40, 20))
        {
            (A, B, C, D, E) =
                ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + ((B & C) | (B & D) | (C & D)) + E + w + C2,
                A,
                Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2),
                C,
                D);
        }
        foreach (var w in W.AsSpan().Slice(60, 20))
        {
            (A, B, C, D, E) =
                ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + (B ^ C ^ D) + E + w + C3,
                A,
                Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2),
                C,
                D);
        }

        var h32 = H1 + B;
        var l32 = H0 + A;
        var sum = 0ul;
        for (int i = 0; i < 8; i++)
        {
            sum += ((ulong)ReverseEndianness(h32[i]) << 32) | ReverseEndianness(l32[i]);
        }

        return sum;
    }

}

class InitialSeedGenerator3
{
    const uint H0 = 0x67452301;
    const uint H1 = 0xEFCDAB89;
    const uint H2 = 0x98BADCFE;
    const uint H3 = 0x10325476;
    const uint H4 = 0xC3D2E1F0;

    const int N = 80;

    private readonly uint[] W = new uint[N];
    public InitialSeedGenerator3(uint[] mac, uint[] nazo, uint v, uint frame, uint t0)
    {
        W[0] = ReverseEndianness(nazo[0]);
        W[1] = ReverseEndianness(nazo[1]);
        W[2] = ReverseEndianness(nazo[2]);
        W[3] = ReverseEndianness(nazo[3]);
        W[4] = ReverseEndianness(nazo[4]);

        W[5] = ReverseEndianness((v << 16) | t0);
        W[6] = (mac[4] << 8) | mac[5];
        W[7] = ReverseEndianness(0x6000000 ^ frame ^ (mac[3] << 24) | (mac[2] << 16) | (mac[1] << 8) | (mac[0]));

        W[10] = 0x00000000;
        W[11] = 0x00000000;
        W[12] = 0xFF2F0000;
        W[13] = 0x80000000;
        W[14] = 0x00000000;
        W[15] = 0x000001A0;
    }

    public uint GenerateMTSeed(uint dateCode, uint timeCode)
    {
        W[8] = dateCode;
        W[9] = timeCode;

        for (int t = 16; t < N; t++)
        {
            var w = W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16];
            W[t] = (w << 1) | (w >> 31);
        }

        var A = H0;
        var B = H1;
        var C = H2;
        var D = H3;
        var E = H4;

        for (int t = 0; t < 20; t++)
        {
            var temp = BitOperations.RotateLeft(A, 5) + ((B & C) | ((~B) & D)) + E + W[t] + 0x5A827999;
            E = D;
            D = C;
            C = (B << 30) | (B >> 2);
            B = A;
            A = temp;
        }
        for (int t = 20; t < 40; t++)
        {
            var temp = BitOperations.RotateLeft(A, 5) + (B ^ C ^ D) + E + W[t] + 0x6ED9EBA1;
            E = D;
            D = C;
            C = (B << 30) | (B >> 2);
            B = A;
            A = temp;
        }
        for (int t = 40; t < 60; t++)
        {
            var temp = BitOperations.RotateLeft(A, 5) + ((B & C) | (B & D) | (C & D)) + E + W[t] + 0x8F1BBCDC;
            E = D;
            D = C;
            C = (B << 30) | (B >> 2);
            B = A;
            A = temp;
        }
        for (int t = 60; t < N; t++)
        {
            var temp = BitOperations.RotateLeft(A, 5) + (B ^ C ^ D) + E + W[t] + 0xCA62C1D6;
            E = D;
            D = C;
            C = (B << 30) | (B >> 2);
            B = A;
            A = temp;
        }

        var seed = (((ulong)ReverseEndianness(H1 + B) << 32) | ReverseEndianness(H0 + A));

        return (uint)((seed * 0x5D588B656C078965UL + 0x269EC3UL) >> 32);
    }

}


class InitialSeedGeneratorBase
{
    const uint H0 = 0x67452301;
    const uint H1 = 0xEFCDAB89;
    const uint H2 = 0x98BADCFE;
    const uint H3 = 0x10325476;
    const uint H4 = 0xC3D2E1F0;

    private readonly uint[] W = new uint[80];
    public InitialSeedGeneratorBase(uint[] mac, uint[] nazo, uint v, uint frame, uint t0)
    {
        W[0] = ReverseEndianness(nazo[0]);
        W[1] = ReverseEndianness(nazo[1]);
        W[2] = ReverseEndianness(nazo[2]);
        W[3] = ReverseEndianness(nazo[3]);
        W[4] = ReverseEndianness(nazo[4]);

        W[5] = ReverseEndianness((v << 16) | t0);
        W[6] = (mac[4] << 8) | mac[5];
        W[7] = ReverseEndianness(0x6000000 ^ frame ^ (mac[3] << 24) | (mac[2] << 16) | (mac[1] << 8) | (mac[0]));

        W[10] = 0x00000000;
        W[11] = 0x00000000;
        W[12] = 0xFF2F0000;
        W[13] = 0x80000000;
        W[14] = 0x00000000;
        W[15] = 0x000001A0;
    }

    public uint GenerateMTSeed(uint dateCode, uint timeCode)
    {
        W[8] = dateCode;
        W[9] = timeCode;

        for (int t = 16; t < 80; t++)
        {
            var w = W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16];
            W[t] = (w << 1) | (w >> 31);
        }

        var A = H0;
        var B = H1;
        var C = H2;
        var D = H3;
        var E = H4;

        for (int t = 0; t < 20; t++)
        {
            var temp = ((A << 5) | (A >> 27)) + ((B & C) | ((~B) & D)) + E + W[t] + 0x5A827999;
            E = D;
            D = C;
            C = (B << 30) | (B >> 2);
            B = A;
            A = temp;
        }
        for (int t = 20; t < 40; t++)
        {
            var temp = ((A << 5) | (A >> 27)) + (B ^ C ^ D) + E + W[t] + 0x6ED9EBA1;
            E = D;
            D = C;
            C = (B << 30) | (B >> 2);
            B = A;
            A = temp;
        }
        for (int t = 40; t < 60; t++)
        {
            var temp = ((A << 5) | (A >> 27)) + ((B & C) | (B & D) | (C & D)) + E + W[t] + 0x8F1BBCDC;
            E = D;
            D = C;
            C = (B << 30) | (B >> 2);
            B = A;
            A = temp;
        }
        for (int t = 60; t < 80; t++)
        {
            var temp = ((A << 5) | (A >> 27)) + (B ^ C ^ D) + E + W[t] + 0xCA62C1D6;
            E = D;
            D = C;
            C = (B << 30) | (B >> 2);
            B = A;
            A = temp;
        }

        var seed = (((ulong)ReverseEndianness(H1 + B) << 32) | ReverseEndianness(H0 + A));

        return (uint)((seed * 0x5D588B656C078965UL + 0x269EC3UL) >> 32);
    }
    public ulong Generate(uint dateCode, uint timeCode)
    {
        W[8] = dateCode;
        W[9] = timeCode;

        uint t;
        for (t = 16; t < 80; t++)
        {
            var w = W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16];
            W[t] = (w << 1) | (w >> 31);
        }

        const uint H0 = 0x67452301;
        const uint H1 = 0xEFCDAB89;
        const uint H2 = 0x98BADCFE;
        const uint H3 = 0x10325476;
        const uint H4 = 0xC3D2E1F0;

        uint A, B, C, D, E;
        A = H0; B = H1; C = H2; D = H3; E = H4;

        for (t = 0; t < 20; t++)
        {
            var temp = ((A << 5) | (A >> 27)) + ((B & C) | ((~B) & D)) + E + W[t] + 0x5A827999;
            E = D;
            D = C;
            C = (B << 30) | (B >> 2);
            B = A;
            A = temp;
        }
        for (; t < 40; t++)
        {
            var temp = ((A << 5) | (A >> 27)) + (B ^ C ^ D) + E + W[t] + 0x6ED9EBA1;
            E = D;
            D = C;
            C = (B << 30) | (B >> 2);
            B = A;
            A = temp;
        }
        for (; t < 60; t++)
        {
            var temp = ((A << 5) | (A >> 27)) + ((B & C) | (B & D) | (C & D)) + E + W[t] + 0x8F1BBCDC;
            E = D;
            D = C;
            C = (B << 30) | (B >> 2);
            B = A;
            A = temp;
        }
        for (; t < 80; t++)
        {
            var temp = ((A << 5) | (A >> 27)) + (B ^ C ^ D) + E + W[t] + 0xCA62C1D6;
            E = D;
            D = C;
            C = (B << 30) | (B >> 2);
            B = A;
            A = temp;
        }

        ulong seed = ReverseEndianness(H1 + B);
        seed <<= 32;
        seed |= ReverseEndianness(H0 + A);

        return seed * 0x5D588B656C078965UL + 0x269EC3UL;
    }

}



class InitialSeedGenerator__
{
    private readonly Vector256<uint> H0 = Vector256.Create(0x67452301u);
    private readonly Vector256<uint> H1 = Vector256.Create(0xEFCDAB89);
    private readonly Vector256<uint> H2 = Vector256.Create(0x98BADCFE);
    private readonly Vector256<uint> H3 = Vector256.Create(0x10325476u);
    private readonly Vector256<uint> H4 = Vector256.Create(0xC3D2E1F0);

    private readonly Vector256<uint> C0 = Vector256.Create(0x5A827999u);
    private readonly Vector256<uint> C1 = Vector256.Create(0x6ED9EBA1u);
    private readonly Vector256<uint> C2 = Vector256.Create(0x8F1BBCDCu);
    private readonly Vector256<uint> C3 = Vector256.Create(0xCA62C1D6u);

    const int N = 80;

    private readonly Vector256<uint>[] W = new Vector256<uint>[N];
    public readonly Vector256<uint>[] dateCodes = new Vector256<uint>[36525];
    private readonly Vector256<uint>[] timeCodes = new Vector256<uint>[86400 / 8];

    public InitialSeedGenerator__(uint[] mac, uint[] nazo, uint v, uint frame, uint t0)
    {
        W[0] = Vector256.Create(ReverseEndianness(nazo[0]));
        W[1] = Vector256.Create(ReverseEndianness(nazo[1]));
        W[2] = Vector256.Create(ReverseEndianness(nazo[2]));
        W[3] = Vector256.Create(ReverseEndianness(nazo[3]));
        W[4] = Vector256.Create(ReverseEndianness(nazo[4]));

        W[5] = Vector256.Create(ReverseEndianness((v << 16) | t0));
        W[6] = Vector256.Create((mac[4] << 8) | mac[5]);
        W[7] = Vector256.Create(ReverseEndianness(0x6000000 ^ frame ^ (mac[3] << 24) | (mac[2] << 16) | (mac[1] << 8) | (mac[0])));

        W[10] = Vector256<uint>.Zero;
        W[11] = Vector256<uint>.Zero;
        W[12] = Vector256.Create(0xFF2F0000);
        W[13] = Vector256.Create(0x80000000);
        W[14] = Vector256<uint>.Zero;
        W[15] = Vector256.Create(0x000001A0u);

        {
            var i = 0;
            var container = new uint[8];
            for (uint hour = 0; hour < 24; ++hour)
            {
                var h_code = ((hour / 10) << 28) | ((hour % 10) << 24);
                if (hour >= 12) h_code |= 0x40000000;
                for (uint minute = 0; minute < 60; ++minute)
                {
                    var min_code = ((minute / 10) << 20) | ((minute % 10) << 16);
                    for (uint second = 0; second < 60; ++second)
                    {
                        container[i++] = h_code | min_code | ((second / 10) << 12) | ((second % 10) << 8);
                        if (i == 8)
                        {
                            timeCodes[(second + minute * 60 + hour * 3600) / 8] = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(container.AsSpan()));
                            i = 0;
                        }
                    }
                }
            }
        }

        {
            var i = 0;
            var month_ends = new uint[][] { new uint[] { 0, 32, 29, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 }, new uint[] { 0, 32, 30, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 }, };
            for (uint year = 0; year < 100; year++)
            {
                var month_end = (year % 4) == 0 ? month_ends[1] : month_ends[0];

                var y_code = ((year / 10) << 28) | ((year % 10) << 24);
                var yy = 2000u - 1;
                var day = (yy + (yy / 4) - (yy / 100) + (yy / 400) + ((13 * 13 + 8) / 5) + 1) % 7;
                for (uint month = 1; month < 13; ++month)
                {
                    var m_code = ((month / 10) << 20) | ((month % 10) << 16);
                    for (uint date = 1; date < month_end[month]; ++date)
                    {
                        var d_code = ((date / 10) << 12) | ((date % 10) << 8);

                        dateCodes[i++] = Vector256.Create(y_code | m_code | d_code | day);

                        day++; if (day == 7) day = 0;
                    }
                }
            }
        }
    }

    public ulong GenerateMTSeed()
    {
        var sum = 0ul;

        W[8] = dateCodes[0];
        foreach (var tc in timeCodes)
        {
            W[9] = tc;
            for (int t = 16; t < W.Length; t++)
            {
                var w = W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16];
                W[t] = Vector256.ShiftLeft(w, 1) ^ Vector256.ShiftRightLogical(w, 31);
            }

            var (A, B, C, D, E) = (H0, H1, H2, H3, H4);

            foreach (var w in W.AsSpan().Slice(0, 20))
            {
                (A, B, C, D, E) =
                    ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + ((B & C) | ((~B) & D)) + E + w + C0,
                    A,
                    (Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2)),
                    C,
                    D);
            }

            foreach (var w in W.AsSpan().Slice(20, 20))
            {
                (A, B, C, D, E) =
                    ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + (B ^ C ^ D) + E + w + C1,
                    A,
                    Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2),
                    C,
                    D);
            }
            foreach (var w in W.AsSpan().Slice(40, 20))
            {
                (A, B, C, D, E) =
                    ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + ((B & C) | (B & D) | (C & D)) + E + w + C2,
                    A,
                    Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2),
                    C,
                    D);
            }
            foreach (var w in W.AsSpan().Slice(60, 20))
            {
                (A, B, C, D, E) =
                    ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + (B ^ C ^ D) + E + w + C3,
                    A,
                    Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2),
                    C,
                    D);
            }

            var h32 = H1 + B;
            var l32 = H0 + A;
            for (int i = 0; i < 8; i++)
            {
                var lcgSeed = (ulong)ReverseEndianness(h32[i]) << 32 | ReverseEndianness(l32[i]);
                var seed = (uint)(lcgSeed * 0x5D588B656C078965UL + 0x269EC3UL) >> 32;
                sum += seed;
            }
        }

        return sum;
    }

}

class InitialSeedGenerator2__
{
    private readonly Vector256<uint> H0 = Vector256.Create(0x67452301u);
    private readonly Vector256<uint> H1 = Vector256.Create(0xEFCDAB89);
    private readonly Vector256<uint> H2 = Vector256.Create(0x98BADCFE);
    private readonly Vector256<uint> H3 = Vector256.Create(0x10325476u);
    private readonly Vector256<uint> H4 = Vector256.Create(0xC3D2E1F0);

    private readonly Vector256<uint> C0 = Vector256.Create(0x5A827999u);
    private readonly Vector256<uint> C1 = Vector256.Create(0x6ED9EBA1u);
    private readonly Vector256<uint> C2 = Vector256.Create(0x8F1BBCDCu);
    private readonly Vector256<uint> C3 = Vector256.Create(0xCA62C1D6u);

    const int N = 80;

    private readonly Vector256<uint>[] W = new Vector256<uint>[N];
    public readonly Vector256<uint>[] dateCodes = new Vector256<uint>[36525];
    private readonly Vector256<uint>[] timeCodes = new Vector256<uint>[86400 / 8];

    public InitialSeedGenerator2__(uint[] mac, uint[] nazo, uint v, uint frame, uint t0)
    {
        W[0] = Vector256.Create(ReverseEndianness(nazo[0]));
        W[1] = Vector256.Create(ReverseEndianness(nazo[1]));
        W[2] = Vector256.Create(ReverseEndianness(nazo[2]));
        W[3] = Vector256.Create(ReverseEndianness(nazo[3]));
        W[4] = Vector256.Create(ReverseEndianness(nazo[4]));

        W[5] = Vector256.Create(ReverseEndianness((v << 16) | t0));
        W[6] = Vector256.Create((mac[4] << 8) | mac[5]);
        W[7] = Vector256.Create(ReverseEndianness(0x6000000 ^ frame ^ (mac[3] << 24) | (mac[2] << 16) | (mac[1] << 8) | (mac[0])));

        W[10] = Vector256<uint>.Zero;
        W[11] = Vector256<uint>.Zero;
        W[12] = Vector256.Create(0xFF2F0000);
        W[13] = Vector256.Create(0x80000000);
        W[14] = Vector256<uint>.Zero;
        W[15] = Vector256.Create(0x000001A0u);

        {
            var i = 0;
            var container = new uint[8];
            for (uint hour = 0; hour < 24; ++hour)
            {
                var h_code = ((hour / 10) << 28) | ((hour % 10) << 24);
                if (hour >= 12) h_code |= 0x40000000;
                for (uint minute = 0; minute < 60; ++minute)
                {
                    var min_code = ((minute / 10) << 20) | ((minute % 10) << 16);
                    for (uint second = 0; second < 60; ++second)
                    {
                        container[i++] = h_code | min_code | ((second / 10) << 12) | ((second % 10) << 8);
                        if (i == 8)
                        {
                            timeCodes[(second + minute * 60 + hour * 3600) / 8] = Vector256.LoadUnsafe(ref MemoryMarshal.GetReference(container.AsSpan()));
                            i = 0;
                        }
                    }
                }
            }
        }

        {
            var i = 0;
            var month_ends = new uint[][] { new uint[] { 0, 32, 29, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 }, new uint[] { 0, 32, 30, 32, 31, 32, 31, 32, 32, 31, 32, 31, 32 }, };
            for (uint year = 0; year < 100; year++)
            {
                var month_end = (year % 4) == 0 ? month_ends[1] : month_ends[0];

                var y_code = ((year / 10) << 28) | ((year % 10) << 24);
                var yy = 2000u - 1;
                var day = (yy + (yy / 4) - (yy / 100) + (yy / 400) + ((13 * 13 + 8) / 5) + 1) % 7;
                for (uint month = 1; month < 13; ++month)
                {
                    var m_code = ((month / 10) << 20) | ((month % 10) << 16);
                    for (uint date = 1; date < month_end[month]; ++date)
                    {
                        var d_code = ((date / 10) << 12) | ((date % 10) << 8);

                        dateCodes[i++] = Vector256.Create(y_code | m_code | d_code | day);

                        day++; if (day == 7) day = 0;
                    }
                }
            }
        }
    }

    private readonly Vector256<ulong> MUL = Vector256.Create(0x5D588B656C078965UL);
    private readonly Vector256<ulong> ADD = Vector256.Create(0x269EC3UL);
    public ulong GenerateMTSeed()
    {
        var sum = 0ul;

        W[8] = dateCodes[0];
        foreach (var tc in timeCodes)
        {
            W[9] = tc;
            for (int t = 16; t < W.Length; t++)
            {
                var w = W[t - 3] ^ W[t - 8] ^ W[t - 14] ^ W[t - 16];
                W[t] = Vector256.ShiftLeft(w, 1) ^ Vector256.ShiftRightLogical(w, 31);
            }

            var (A, B, C, D, E) = (H0, H1, H2, H3, H4);

            foreach (var w in W.AsSpan().Slice(0, 20))
            {
                (A, B, C, D, E) =
                    ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + ((B & C) | ((~B) & D)) + E + w + C0,
                    A,
                    (Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2)),
                    C,
                    D);
            }

            foreach (var w in W.AsSpan().Slice(20, 20))
            {
                (A, B, C, D, E) =
                    ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + (B ^ C ^ D) + E + w + C1,
                    A,
                    Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2),
                    C,
                    D);
            }
            foreach (var w in W.AsSpan().Slice(40, 20))
            {
                (A, B, C, D, E) =
                    ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + ((B & C) | (B & D) | (C & D)) + E + w + C2,
                    A,
                    Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2),
                    C,
                    D);
            }
            foreach (var w in W.AsSpan().Slice(60, 20))
            {
                (A, B, C, D, E) =
                    ((Vector256.ShiftLeft(A, 5) ^ Vector256.ShiftRightLogical(A, 27)) + (B ^ C ^ D) + E + w + C3,
                    A,
                    Vector256.ShiftLeft(B, 30) ^ Vector256.ShiftRightLogical(B, 2),
                    C,
                    D);
            }

            var shuffle = Vector256.Create(0x0C0D0E0Fu, 0x08090A0B, 0x04050607, 0x00010203, 0x0C0D0E0F, 0x08090A0B, 0x04050607, 0x00010203);

            var (h1, h2) = Vector256.Widen(Vector256.Shuffle(H1 + B, shuffle));
            var (l1, l2) = Vector256.Widen(Vector256.Shuffle(H0 + A, shuffle));

            var lcgSeed1 = Vector256.ShiftLeft(h1, 32) | l1;
            var seed1 = Vector256.ShiftRightLogical(lcgSeed1 * MUL + ADD, 32);
            for (int i = 0; i < 4; i++)
            {
                var seed = (uint)seed1[i];
                sum += seed;
            }

            var lcgSeed2 = Vector256.ShiftLeft(h2, 32) | l2;
            var seed2 = Vector256.ShiftRightLogical(lcgSeed2 * MUL + ADD, 32);
            for (int i = 0; i < 4; i++)
            {
                var seed = (uint)seed2[i];
                sum += seed;
            }
        }

        return sum;
    }

}

static class SIMDExt
{
    private static readonly Vector256<uint> MASK_LEFT = Vector256.Create(0xFF00FF00u);
    private static readonly Vector256<uint> MASK_RIGHT = Vector256.Create(0x00FF00FFu);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector256<uint> ReverseIndianess(in this Vector256<uint> vector)
    {
        var v1 = vector & MASK_RIGHT;
        var v2 = vector & MASK_LEFT;

        return 
            Vector256.ShiftRightLogical(v1, 8) 
            | Vector256.ShiftLeft(v1, 24) 
            | Vector256.ShiftLeft(v2, 8) 
            | Vector256.ShiftRightLogical(v2, 24);
    }
}
