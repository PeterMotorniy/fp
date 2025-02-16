﻿using System;

namespace TagsCloudContainer
{
    public class None
    {
    }

    public readonly struct Result<T>
    {
        public Result(string error, T value = default(T))
        {
            Error = error;
            Value = value;
        }

        public string Error { get; }
        internal T Value { get; }

        public T GetValueOrThrow()
        {
            if (IsSuccess) return Value;
            throw new InvalidOperationException($"No value. Only Error {Error}");
        }

        public bool IsSuccess => Error == null;
    }

    public static class Result
    {
        public static Result<T> AsResult<T>(this T value)
        {
            return Ok(value);
        }

        public static Result<T> Ok<T>(T value)
        {
            return new Result<T>(null, value);
        }

        public static Result<T> Fail<T>(string e)
        {
            return new Result<T>(e);
        }

        public static Result<T> Of<T>(Func<T> f, string error = null)
        {
            try
            {
                return Ok(f());
            }
            catch (Exception e)
            {
                return Fail<T>(error ?? e.Message);
            }
        }

        public static Result<TOutput> Then<TInput, TOutput>(this Result<TInput> input,
            Func<TInput, TOutput> continuation)
        {
            if (!input.IsSuccess) return Fail<TOutput>(input.Error);
            try
            {
                return new Result<TOutput>(null, continuation(input.Value));
            }
            catch (Exception e)
            {
                return Fail<TOutput>(e.Message);
            }
        }

        public static Result<TInput> ThenDo<TInput>(this Result<TInput> input, Func<TInput, Result<None>> action)
        {
            if (!input.IsSuccess) return Fail<TInput>(input.Error);
            try
            {
                return !action.Invoke(input.Value).IsSuccess ? Fail<TInput>(action.Invoke(input.Value).Error) : input;
            }
            catch (Exception e)
            {
                return Fail<TInput>(e.Message);
            }
        }

        public static Result<TInput> ThenDo<TInput>(this Result<TInput> input, Action<TInput> action)
        {
            if (!input.IsSuccess) return Fail<TInput>(input.Error);
            try
            {
                action.Invoke(input.Value);
                return input;
            }
            catch (Exception e)
            {
                return Fail<TInput>(e.Message);
            }
        }

        public static Result<TOutput> Then<TInput, TOutput>(this Result<TInput> input,
            Func<TInput, Result<TOutput>> continuation) =>
            continuation(input.Value);

        public static Result<TInput> OnFail<TInput>(this Result<TInput> input, Action<string> handleError)
        {
            if (input.IsSuccess) return input;
            handleError(input.Error);
            return Fail<TInput>(input.Error);
        }

        public static Result<TInput> ReplaceError<TInput>(this Result<TInput> input, Func<string, string> func) =>
            input.IsSuccess ? input : Fail<TInput>(func(input.Error));

        public static Result<TInput> RefineError<TInput>(this Result<TInput> input, string error) =>
            Fail<TInput>($"{error}. {input.Error}");
    }
}