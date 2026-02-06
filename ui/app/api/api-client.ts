import { type Result, type ApiError, success, failure } from "./result";

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
        const error: ApiError = {
          status: response.status,
          message: await this.extractErrorMessage(response),
        };
        return failure(error);
      }

      if (response.status === 204) {
        return success(undefined as T);
      }

      const data = (await response.json()) as T;
      return success(data);
    } catch (e) {
      const error: ApiError = {
        status: 0,
        message:
          e instanceof Error ? e.message : "An unknown network error occurred",
      };
      return failure(error);
    }
  }

  private async extractErrorMessage(response: Response): Promise<string> {
    try {
      const text = await response.text();
      try {
        const json = JSON.parse(text) as Record<string, unknown>;
        if (typeof json === "object" && json !== null) {
          if (typeof json.title === "string") return json.title;
          if (typeof json.detail === "string") return json.detail;
          if ("errors" in json) return JSON.stringify(json.errors);
        }
        return text;
      } catch {
        return text || response.statusText;
      }
    } catch {
      return response.statusText;
    }
  }
}

