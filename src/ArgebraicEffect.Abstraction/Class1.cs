namespace ArgebraicEffect.Abstraction;

using LanguageExt.Effects.Traits;

public interface IHas<T> : HasCancel<T> where T : struct, IHas<T>
{

}
