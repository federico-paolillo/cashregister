export type ApiError = {
  status: number;
  message: string;
};

export type Result<T> =
  | { ok: true; value: T }
  | { ok: false; error: ApiError };

export function success<T>(value: T): Result<T> {
  return { ok: true, value };
}

export function failure<T>(error: ApiError): Result<T> {
  return { ok: false, error };
}
