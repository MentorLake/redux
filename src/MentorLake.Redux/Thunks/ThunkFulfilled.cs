namespace MentorLake.Redux.Thunks;

public record ThunkFulfilled<T, TResult>(TResult Result) : ThunkFulfilled<T>;

public record ThunkFulfilled<T>();
