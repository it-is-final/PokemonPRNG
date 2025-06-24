# PokemonPRNG.TinyMT

TinyMT (Tiny Mersenne Twister) の実装です。

## API Reference

### TinyMTクラス

#### コンストラクタ

```csharp
TinyMT(uint seed)
```

**説明**: 指定したseedでTinyMTインスタンスを初期化します。  
**パラメータ**:
- `seed`: 初期seed値

#### GetRand

```csharp
uint GetRand()
```

**説明**: 32bit乱数を生成し、状態を進めます。  
**戻り値**: 32bit乱数

#### Advance

```csharp
void Advance()
void Advance(uint n)
```

**説明**: 状態を指定したステップ数だけ進めます。  
**パラメータ**:
- `n`: 進めるステップ数（省略時は1）

#### Clone

```csharp
TinyMT Clone()
```

**説明**: 現在の状態を複製した新しいTinyMTインスタンスを作成します。  
**戻り値**: 複製されたTinyMTインスタンス

### 拡張メソッド

#### TinyMT

クラスではなく32bit整数4要素のタプルで状態を扱う方法を提供します。

```csharp
(uint, uint, uint, uint) TinyMT(this uint seed)
```

**説明**: 32bit seedからTinyMT状態タプルを初期化します。  
**パラメータ**:
- `seed`: 初期seed値

**戻り値**: TinyMT状態タプル（S0, S1, S2, S3）

#### GetRand

```csharp
uint GetRand(ref this (uint, uint, uint, uint) state)
```

**説明**: 状態タプルから乱数を生成し、状態を更新します。  
**パラメータ**:
- `state`: TinyMT状態タプル

**戻り値**: 32bit乱数

#### Next

```csharp
(uint, uint, uint, uint) Next(this (uint, uint, uint, uint) state)
(uint, uint, uint, uint) Next(this (uint, uint, uint, uint) state, uint n)
```

**説明**: 指定したステップ数だけ進んだ状態タプルを返します。元の状態は変更されません。  
**パラメータ**:
- `state`: TinyMT状態タプル
- `n`: 進めるステップ数（省略時は1）

**戻り値**: 進んだ後の状態タプル

#### Prev

```csharp
(uint, uint, uint, uint) Prev(this (uint, uint, uint, uint) state)
(uint, uint, uint, uint) Prev(this (uint, uint, uint, uint) state, uint n)
```

**説明**: 指定したステップ数だけ戻った状態タプルを返します。元の状態は変更されません。  
**パラメータ**:
- `state`: TinyMT状態タプル
- `n`: 戻すステップ数（省略時は1）

**戻り値**: 戻った後の状態タプル

#### Advance

```csharp
void Advance(ref this (uint, uint, uint, uint) state)
void Advance(ref this (uint, uint, uint, uint) state, uint n)
```

**説明**: 状態タプルを指定したステップ数だけ進めます。状態が変更されます。  
**パラメータ**:
- `state`: TinyMT状態タプル
- `n`: 進めるステップ数（省略時は1）

#### Back

```csharp
void Back(ref this (uint, uint, uint, uint) state)
void Back(ref this (uint, uint, uint, uint) state, uint n)
```

**説明**: 状態タプルを指定したステップ数だけ戻します。状態が変更されます。  
**パラメータ**:
- `state`: TinyMT状態タプル
- `n`: 戻すステップ数（省略時は1）

## システム要件

- .NET Standard 2.0以降

