# PokemonPRNG.XorShift128

XorShift128の実装です。

## API Reference

### 拡張メソッド

32bit整数4要素のタプルでXorShift128の状態を扱う方法を提供します。

#### GetRand

```csharp
uint GetRand(ref this (uint S0, uint S1, uint S2, uint S3) state)
uint GetRand(ref this (uint S0, uint S1, uint S2, uint S3) state, uint n)
```

**説明**: 状態タプルから乱数を生成し、状態を更新します。  
**パラメータ**:
- `state`: XorShift128状態タプル
- `n`: 乱数の範囲（最大値、排他）

**戻り値**: 32bit乱数

#### GetRand_f

```csharp
float GetRand_f(ref this (uint S0, uint S1, uint S2, uint S3) state)
float GetRand_f(ref this (uint S0, uint S1, uint S2, uint S3) state, float min, float max)
```

**説明**: 状態タプルから浮動小数点乱数を生成し、状態を更新します。  
**パラメータ**:
- `state`: XorShift128状態タプル
- `min`: 下限値
- `max`: 上限値

**戻り値**: 0.0-1.0範囲の浮動小数点乱数、または指定範囲の浮動小数点乱数

#### Next

```csharp
(uint S0, uint S1, uint S2, uint S3) Next(this (uint S0, uint S1, uint S2, uint S3) state)
(uint S0, uint S1, uint S2, uint S3) Next(this (uint S0, uint S1, uint S2, uint S3) state, uint n)
(uint S0, uint S1, uint S2, uint S3) Next(this (uint S0, uint S1, uint S2, uint S3) state, ulong n)
```

**説明**: 指定したステップ数だけ進んだ状態タプルを返します。元の状態は変更されません。  
**パラメータ**:
- `state`: XorShift128状態タプル
- `n`: 進めるステップ数（省略時は1）

**戻り値**: 進んだ後の状態タプル

#### Prev

```csharp
(uint S0, uint S1, uint S2, uint S3) Prev(this (uint S0, uint S1, uint S2, uint S3) state)
(uint S0, uint S1, uint S2, uint S3) Prev(this (uint S0, uint S1, uint S2, uint S3) state, uint n)
(uint S0, uint S1, uint S2, uint S3) Prev(this (uint S0, uint S1, uint S2, uint S3) state, ulong n)
```

**説明**: 指定したステップ数だけ戻った状態タプルを返します。元の状態は変更されません。  
**パラメータ**:
- `state`: XorShift128状態タプル
- `n`: 戻すステップ数（省略時は1）

**戻り値**: 戻った後の状態タプル

#### Advance

```csharp
(uint S0, uint S1, uint S2, uint S3) Advance(ref this (uint S0, uint S1, uint S2, uint S3) state)
(uint S0, uint S1, uint S2, uint S3) Advance(ref this (uint S0, uint S1, uint S2, uint S3) state, uint n)
(uint S0, uint S1, uint S2, uint S3) Advance(ref this (uint S0, uint S1, uint S2, uint S3) state, ulong n)
```

**説明**: 状態タプルを指定したステップ数だけ進めます。状態が変更されます。  
**パラメータ**:
- `state`: XorShift128状態タプル
- `n`: 進めるステップ数（省略時は1）

**戻り値**: 更新された状態タプル

#### Back

```csharp
(uint S0, uint S1, uint S2, uint S3) Back(ref this (uint S0, uint S1, uint S2, uint S3) state)
(uint S0, uint S1, uint S2, uint S3) Back(ref this (uint S0, uint S1, uint S2, uint S3) state, uint n)
(uint S0, uint S1, uint S2, uint S3) Back(ref this (uint S0, uint S1, uint S2, uint S3) state, ulong n)
```

**説明**: 状態タプルを指定したステップ数だけ戻します。状態が変更されます。  
**パラメータ**:
- `state`: XorShift128状態タプル
- `n`: 戻すステップ数（省略時は1）

**戻り値**: 更新された状態タプル

### 文字列変換

#### ToU128String

```csharp
string ToU128String(this (uint S0, uint S1, uint S2, uint S3) state)
```

**説明**: 状態タプルを16進文字列に変換します。  
**パラメータ**:
- `state`: XorShift128状態タプル

**戻り値**: 32文字の16進文字列

#### FromU128String

```csharp
(uint S0, uint S1, uint S2, uint S3) FromU128String(this string hex)
```

**説明**: 16進文字列から状態タプルを復元します。  
**パラメータ**:
- `hex`: 16進文字列（最大32文字）

**戻り値**: XorShift128状態タプル

### 列挙

#### Enumerate

```csharp
IEnumerable<(uint S0, uint S1, uint S2, uint S3)> Enumerate(this (uint S0, uint S1, uint S2, uint S3) seed)
```

**説明**: 指定されたseedから始まる状態を列挙します。  
**戻り値**: 状態値のシーケンス

## インターフェース

### IGeneratable<TResult>

生成処理の純粋関数インターフェース

### IGeneratableEffectful<TResult>

状態更新を伴う生成処理インターフェース

## システム要件

- .NET Standard 2.0以降

