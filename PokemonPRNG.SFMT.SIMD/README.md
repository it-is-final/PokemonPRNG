# PokemonPRNG.SFMT.SIMD

SFMTのSIMD最適化された実装です。

## API Reference

### SIMDSFMTクラス

#### コンストラクタ

```csharp
SIMDSFMT(uint seed)
```

**説明**: 指定したseedでSIMDSFMTインスタンスを初期化します。実行時にCPU機能を自動検出し、最適なSIMD実装を選択します。  
**パラメータ**:
- `seed`: 初期seed値

#### GetRand32

```csharp
uint GetRand32()
```

**説明**: SIMD最適化された32bit乱数を生成し、状態を進めます。  
**戻り値**: 生成された32bit乱数

#### GetRand64

```csharp
ulong GetRand64()
```

**説明**: SIMD最適化された64bit乱数を生成し、状態を進めます。  
**戻り値**: 生成された64bit乱数

#### Index

```csharp
ulong Index { get; }
```

**説明**: 現在の状態のインデックス（消費数）を取得します。  
**戻り値**: 消費数

### CachedSIMDSFMTクラス

乱数値をプールして高速にアクセスできるSFMTの実装です。
乱数を複数回使う処理を1消費ずつ走査していく場合に利用するとパフォーマンス向上が見込めます。
キャッシュサイズを超える範囲にアクセスしようとした場合、デバッグビルドでは例外が送出されますが、リリースビルドでは正しくない計算結果が返って動き続けることに注意してください。

#### コンストラクタ

```csharp
CachedSIMDSFMT(uint seed, int cacheSize)
```

**説明**: 指定したseedとキャッシュサイズでCachedSIMDSFMTインスタンスを初期化します。  
**パラメータ**:
- `seed`: 初期seed値
- `cacheSize`: キャッシュサイズ（1-100セグメント）

#### GetRand32

```csharp
uint GetRand32()
```

**説明**: キャッシュされた32bit乱数を取得し、状態を進めます。  
**戻り値**: 32bit乱数

#### GetRand64

```csharp
ulong GetRand64()
```

**説明**: キャッシュされた64bit乱数を取得し、状態を進めます。  
**戻り値**: 64bit乱数

#### MoveNext

```csharp
void MoveNext()
```

**説明**: キャッシュの基準位置を1つ進めます。

#### Advance

```csharp
void Advance(uint n)
```

**説明**: キャッシュ内で状態を指定したステップ数だけ進めます。  
**パラメータ**:
- `n`: 進めるステップ数

## SIMD対応

| 命令セット | パフォーマンス | 対応CPU |
|-----------|--------------|----------|
| **AVX2** | 最高速（256bit並列） | Intel Haswell以降 |
| **SSE2** | 高速（128bit並列） | 2001年以降のほぼ全てのx86 CPU |
| **スカラー** | 通常（フォールバック） | 全プラットフォーム |

## システム要件

- .NET 7.0以降
- x64/x86プロセッサ対応
- AVX2/SSE2対応CPU推奨


