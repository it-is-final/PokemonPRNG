# PokemonPRNG.LCG32

32bit LCG (線形合同法) の実装です。

## パッケージ構成

| パッケージ | LCGパラメータ | 対応ゲーム |
|-----------|-------------|----------|
| **Core** | - | 共通インターフェース・基盤実装 |
| **StandardLCG** | `a=0x41C64E6D, c=0x6073` | 第3世代・第4世代 |
| **GCLCG** | `a=0x343FD, c=0x269EC3` | コロシアム・XD |
| **StaticLCG** | `a=0x41C64E6D, c=0x3039` | IDくじ等 |
| **AlternativeLCG** | `a=0x6C078965, c=1` | 代替実装 |

## API Reference

### Seed操作（拡張メソッド）

#### NextSeed

```csharp
uint NextSeed(this uint seed)
uint NextSeed(this uint seed, uint n)
```

**説明**: 指定したステップ数だけ進んだseed値を返します。元のseedは変更されません。  
**パラメータ**:
- `seed`: 現在のseed値
- `n`: 進めるステップ数（省略時は1）

**戻り値**: 進んだ後のseed値

#### PrevSeed

```csharp
uint PrevSeed(this uint seed)
uint PrevSeed(this uint seed, uint n)
```

**説明**: 指定したステップ数だけ戻ったseed値を返します。元のseedは変更されません。  
**パラメータ**:
- `seed`: 現在のseed値
- `n`: 戻すステップ数（省略時は1）

**戻り値**: 戻った後のseed値

#### Advance

```csharp
void Advance(ref this uint seed)
void Advance(ref this uint seed, uint n)
```

**説明**: seedを指定したステップ数だけ進めます。seedが変更されます。  
**パラメータ**:
- `seed`: 変更するseed値（ref）
- `n`: 進めるステップ数（省略時は1）

#### Back

```csharp
void Back(ref this uint seed)
void Back(ref this uint seed, uint n)
```

**説明**: seedを指定したステップ数だけ戻します。seedが変更されます。  
**パラメータ**:
- `seed`: 変更するseed値（ref）
- `n`: 戻すステップ数（省略時は1）

### 乱数生成

#### GetRand

```csharp
uint GetRand(this uint seed)
uint GetRand(this uint seed, uint max)
```

**説明**: 現在のseedから乱数を生成します。  
**パラメータ**:
- `seed`: 乱数生成に使用するseed値
- `max`: 乱数の上限値（省略時は上位16bitを使用）

**戻り値**: 生成された乱数値

### インデックス計算

#### GetIndex

```csharp
uint GetIndex(this uint seed)
uint GetIndex(this uint seed, uint initialSeed)
```

**説明**: seedの消費数（インデックス）を計算します。  
**パラメータ**:
- `seed`: 現在のseed値
- `initialSeed`: 基準となる初期seed値（省略時は0）

**戻り値**: 消費数

### 列挙

#### EnumerateSeed

```csharp
IEnumerable<uint> EnumerateSeed(this uint seed)
```

**説明**: 指定されたseedから始まるseed値を列挙します。  
**戻り値**: seed値のシーケンス

#### EnumerateRand

```csharp
IEnumerable<uint> EnumerateRand(this uint seed)
```

**説明**: 指定されたseedから始まる乱数値を列挙します。  
**戻り値**: 乱数値のシーケンス

#### Surround

```csharp
IEnumerable<uint> Surround(this uint seed, int range)
```

**説明**: 指定されたseedの前後の範囲のseed値を列挙します。  
**パラメータ**:
- `seed`: 中心となるseed値
- `range`: 前後の範囲

**戻り値**: 周辺のseed値のシーケンス

## インターフェース

### IGeneratable<TResult>

生成処理の純粋関数インターフェース（最大4引数対応）

### IGeneratableEffectful<TResult>

seed更新を伴う生成処理インターフェース

### ILcgUser

基本的なseed消費処理インターフェース

### ILcgConsumer

消費量計算が可能なインターフェース

### ICriteria<T>

条件判定用インターフェース

## システム要件

- .NET Standard 2.0以降
