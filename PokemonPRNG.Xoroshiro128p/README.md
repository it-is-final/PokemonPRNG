# PokemonPRNG.Xoroshiro128p

Xoroshiro128+の実装です。

## API Reference

### 拡張メソッド

64bit整数2要素のタプルでXoroshiro128+の状態を扱う方法を提供します。

#### GetRand

```csharp
ulong GetRand(ref this (ulong, ulong) state)
```

**説明**: 状態タプルから64bit乱数を生成し、状態を更新します。  
**パラメータ**:
- `state`: Xoroshiro128+状態タプル

**戻り値**: 64bit乱数

#### GetRand32

```csharp
uint GetRand32(ref this (ulong, ulong) state)
```

**説明**: 状態タプルから32bit乱数を生成し、状態を更新します。  
**パラメータ**:
- `state`: Xoroshiro128+状態タプル

**戻り値**: 32bit乱数

#### GetRand (range)

```csharp
uint GetRand(ref this (ulong, ulong) state, uint max)
uint GetRand(ref this (ulong, ulong) state, uint min, uint max)
```

**説明**: 指定した範囲の乱数を生成し、状態を更新します。  
**パラメータ**:
- `state`: Xoroshiro128+状態タプル
- `max`: 上限値
- `min`: 下限値

**戻り値**: 指定範囲の乱数

#### Next

```csharp
(ulong, ulong) Next(this (ulong, ulong) state)
(ulong, ulong) Next(this (ulong, ulong) state, ulong n)
```

**説明**: 指定したステップ数だけ進んだ状態タプルを返します。元の状態は変更されません。  
**パラメータ**:
- `state`: Xoroshiro128+状態タプル
- `n`: 進めるステップ数（省略時は1）

**戻り値**: 進んだ後の状態タプル

#### Advance

```csharp
void Advance(ref this (ulong, ulong) state)
void Advance(ref this (ulong, ulong) state, ulong n)
```

**説明**: 状態タプルを指定したステップ数だけ進めます。状態が変更されます。  
**パラメータ**:
- `state`: Xoroshiro128+状態タプル
- `n`: 進めるステップ数（省略時は1）

### BDSP向けの機能

#### Initialize

```csharp
(ulong, ulong) Initialize(this uint seed)
```

**説明**: BDSPの実装で、32bit seedからXoroshiro128+状態タプルを返します。  
**パラメータ**:
- `seed`: 32bit初期seed

**戻り値**: Xoroshiro128+状態タプル

## システム要件

- .NET Standard 2.0以降

