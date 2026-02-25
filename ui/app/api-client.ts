import { type Result, success, failure } from "@cashregister/result";

export class ApiClient {
  private readonly baseUrl: string;

  constructor(baseUrl: string) {
    this.baseUrl = baseUrl.replace(/\/+$/, "");
  }

  async get<T>(
    path: string,
    params?: Record<string, string>,
  ): Promise<Result<T>> {
    const url = this.buildUrl(path, params);
    return this.request<T>(url, { method: "GET" });
  }

  async post<T>(path: string, body?: unknown): Promise<Result<T>> {
    const url = this.buildUrl(path);
    return this.request<T>(url, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: body !== undefined ? JSON.stringify(body) : undefined,
    });
  }

  async del(path: string): Promise<Result<void>> {
    const url = this.buildUrl(path);
    return this.request<void>(url, { method: "DELETE" });
  }

  private buildUrl(path: string, params?: Record<string, string>): string {
    const normalizedPath = path.startsWith("/") ? path : `/${path}`;
    const fullPath = `${this.baseUrl}${normalizedPath}`;

    if (params && Object.keys(params).length > 0) {
      const searchParams = new URLSearchParams(params);
      return `${fullPath}?${searchParams.toString()}`;
    }

    return fullPath;
  }

  private async request<T>(
    url: string,
    init: RequestInit,
  ): Promise<Result<T>> {
    try {
      const response = await fetch(url, init);

      if (!response.ok) {
        return failure({ status: response.status, message: url });
      }

      if (response.status === 204) {
        return success(undefined as T);
      }

      const data = (await response.json()) as T;
      return success(data);
    } catch (e) {
      return failure({
        status: 0,
        message:
          e instanceof Error ? e.message : "An unknown network error occurred",
      });
    }
  }
}

