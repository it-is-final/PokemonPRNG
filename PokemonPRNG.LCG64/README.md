# PokemonPRNG.LCG64

64bit LCG (線形合同法) の実装です。

## API Reference

### Seed操作（拡張メソッド）

#### NextSeed

```csharp
ulong NextSeed(this ulong seed)
ulong NextSeed(this ulong seed, ulong n)
```

**説明**: 指定したステップ数だけ進んだseed値を返します。元のseedは変更されません。  
**パラメータ**:
- `seed`: 現在のseed値
- `n`: 進めるステップ数（省略時は1）

**戻り値**: 進んだ後のseed値

#### PrevSeed

```csharp
ulong PrevSeed(this ulong seed)
ulong PrevSeed(this ulong seed, ulong n)
```

**説明**: 指定したステップ数だけ戻ったseed値を返します。元のseedは変更されません。  
**パラメータ**:
- `seed`: 現在のseed値
- `n`: 戻すステップ数（省略時は1）

**戻り値**: 戻った後のseed値

#### Advance

```csharp
void Advance(ref this ulong seed)
void Advance(ref this ulong seed, ulong n)
```

**説明**: seedを指定したステップ数だけ進めます。seedが変更されます。  
**パラメータ**:
- `seed`: 変更するseed値（ref）
- `n`: 進めるステップ数（省略時は1）

#### Back

```csharp
void Back(ref this ulong seed)
void Back(ref this ulong seed, ulong n)
```

**説明**: seedを指定したステップ数だけ戻します。seedが変更されます。  
**パラメータ**:
- `seed`: 変更するseed値（ref）
- `n`: 戻すステップ数（省略時は1）

### 乱数生成

#### GetRand

```csharp
uint GetRand(this ulong seed)
uint GetRand(this ulong seed, uint max)
```

**説明**: 現在のseedから乱数を生成します。  
**パラメータ**:
- `seed`: 乱数生成に使用するseed値
- `max`: 乱数の上限値（省略時は上位32bitを使用）

**戻り値**: 生成された乱数値

### インデックス計算

#### GetIndex

```csharp
ulong GetIndex(this ulong seed)
ulong GetIndex(this ulong seed, ulong initialSeed)
```

**説明**: seedの消費数（インデックス）を計算します。  
**パラメータ**:
- `seed`: 現在のseed値
- `initialSeed`: 基準となる初期seed値（省略時は0）

**戻り値**: 消費数

### 列挙

#### EnumerateSeed

```csharp
IEnumerable<ulong> EnumerateSeed(this ulong seed)
```

**説明**: 指定されたseedから始まるseed値を列挙します。  
**戻り値**: seed値のシーケンス

#### EnumerateRand

```csharp
IEnumerable<uint> EnumerateRand(this ulong seed)
```

**説明**: 指定されたseedから始まる乱数値を列挙します。  
**戻り値**: 乱数値のシーケンス

## インターフェース

### IGeneratable<TResult>

生成処理の純粋関数インターフェース（最大4引数対応）

### IGeneratableEffectful<TResult>

seed更新を伴う生成処理インターフェース

### ILcgUser

基本的なseed消費処理インターフェース

### ILcgUtilizer

高度なseed操作インターフェース（非推奨）

## システム要件

- .NET Standard 2.0以降

