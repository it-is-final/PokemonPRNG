# PokemonPRNG.SFMT

SFMT (SIMD-oriented Fast Mersenne Twister) の実装です。

## API Reference

### SFMTクラス

#### コンストラクタ

```csharp
SFMT(uint seed)
```

**説明**: 指定したseedでSFMTインスタンスを初期化します。  
**パラメータ**:
- `seed`: 初期seed値

#### GetRand32

```csharp
uint GetRand32()
```

**説明**: 32bit乱数を生成し、状態を進めます。  
**戻り値**: 生成された32bit乱数

#### GetRand64

```csharp
ulong GetRand64()
```

**説明**: 64bit乱数を生成し、状態を進めます。  
**戻り値**: 生成された64bit乱数

#### Index

```csharp
ulong Index { get; }
```

**説明**: 現在の状態のインデックス（消費数）を取得します。  
**戻り値**: 消費数

#### Clone

```csharp
SFMT Clone()
```

**説明**: 現在の状態を複製した新しいSFMTインスタンスを作成します。  
**戻り値**: 複製されたSFMTインスタンス

### CachedSFMTクラス

乱数値をプールして高速にアクセスできるSFMTの実装です。
乱数を複数回使う処理を1消費ずつ走査していく場合に利用するとパフォーマンス向上が見込めます。
キャッシュサイズを超える範囲にアクセスしようとした場合、デバッグビルドでは例外が送出されますが、リリースビルドでは正しくない計算結果が返って動き続けることに注意してください。

#### コンストラクタ

```csharp
CachedSFMT(uint seed, int cacheSize)
```

**説明**: 指定したseedとキャッシュサイズでCachedSFMTインスタンスを初期化します。  
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

## システム要件

- .NET Standard 2.0以降

