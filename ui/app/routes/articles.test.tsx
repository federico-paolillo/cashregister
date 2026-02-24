import React from "react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import Articles, { clientAction } from "./articles";
import * as reactRouter from "react-router";
import * as errorMessages from "../components/use-error-messages";
import { deps } from "../deps";
import type { ArticlesPageDto } from "../model";
import type { Result } from "../result";
import type { Route } from "./+types/articles";

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof reactRouter>();
  return {
    ...actual,
    useNavigation: vi.fn(),
    useFetcher: vi.fn(),
    Form: ({ children, ...props }: React.FormHTMLAttributes<HTMLFormElement>) =>
      <form {...props}>{children}</form>,
    Link: ({ children, to, ...props }: { children: React.ReactNode; to: string } & React.AnchorHTMLAttributes<HTMLAnchorElement>) =>
      <a href={String(to)} {...props}>{children}</a>,
  };
});

vi.mock("../components/use-error-messages", () => ({
  useErrorMessages: vi.fn(),
}));

vi.mock("../deps", () => ({
  deps: {
    apiClient: {
      get: vi.fn(),
      post: vi.fn(),
      del: vi.fn(),
    },
  },
}));

const mockInitialData: ArticlesPageDto = {
  items: [
    { id: "1", description: "Article 1", price: 10.0 },
    { id: "2", description: "Article 2", price: 20.0 },
  ],
  next: "cursor-1",
  hasNext: true,
};

const mockInitialResult: Result<ArticlesPageDto> = {
  ok: true,
  value: mockInitialData,
};

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function renderArticles(props: any = {}) {
  return render(
    <Articles loaderData={mockInitialResult} {...props} />,
  );
}

describe("Articles Page", () => {

  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(reactRouter.useNavigation).mockReturnValue({
      state: "idle",
    } as reactRouter.Navigation);
    vi.mocked(reactRouter.useFetcher).mockReturnValue({
      state: "idle",
      data: undefined,
      Form: ({ children }: { children: React.ReactNode }) => <form>{children}</form>,
    } as unknown as reactRouter.FetcherWithComponents<unknown>);
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError: vi.fn(),
      dismissError: vi.fn(),
    });

    HTMLDialogElement.prototype.showModal = vi.fn();
    HTMLDialogElement.prototype.close = vi.fn();
  });

  afterEach(() => {
    cleanup();
  });

  it("renders initial articles from loader", () => {
    renderArticles();

    expect(screen.getByText("Article 1")).toBeDefined();
    expect(screen.getByText("Article 2")).toBeDefined();
  });

  it("shows Load More button when page.next is set", () => {
    renderArticles();

    expect(screen.getByRole("button", { name: "Load More" })).toBeDefined();
  });

  it("hides Load More button when page.next is null", () => {
    const data: Result<ArticlesPageDto> = {
      ok: true,
      value: { ...mockInitialData, next: null, hasNext: false },
    };

    renderArticles({ loaderData: data });

    expect(screen.queryByRole("button", { name: "Load More" })).toBeNull();
  });

  it("'New Articles' link navigates without opening the create modal", () => {
    renderArticles();

    const link = screen.getByRole("link", { name: "New Articles" });

    link.click();

    expect(HTMLDialogElement.prototype.showModal).not.toHaveBeenCalled();
  });

  it("shows loading state when navigation is loading", () => {
    vi.mocked(reactRouter.useNavigation).mockReturnValue({
      state: "loading",
    } as reactRouter.Navigation);

    renderArticles();

    expect(screen.getByText("Loading...")).toBeDefined();
    expect(screen.getByRole("button", { name: "Loading..." })).toHaveProperty("disabled", true);
  });
});

function buildFormRequest(fields: Record<string, string>): Route.ClientActionArgs {
  const formData = new FormData();
  for (const [key, value] of Object.entries(fields)) {
    formData.append(key, value);
  }
  return {
    request: new Request("http://localhost/articles", {
      method: "POST",
      body: formData,
    }),
    params: {},
  } as Route.ClientActionArgs;
}

describe("clientAction", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("calls POST /articles/{id} for edit intent", async () => {
    vi.mocked(deps.apiClient.post).mockResolvedValue({ ok: true, value: undefined });

    const args = buildFormRequest({
      intent: "edit",
      articleId: "art-42",
      description: "Updated name",
      priceInCents: "500",
    });

    await clientAction(args);

    expect(deps.apiClient.post).toHaveBeenCalledWith("/articles/art-42", {
      description: "Updated name",
      priceInCents: 500,
    });
  });

  it("calls POST /articles for create intent", async () => {
    vi.mocked(deps.apiClient.post).mockResolvedValue({ ok: true, value: undefined });

    const args = buildFormRequest({
      intent: "create",
      description: "New article",
      priceInCents: "1000",
    });

    await clientAction(args);

    expect(deps.apiClient.post).toHaveBeenCalledWith("/articles", {
      description: "New article",
      priceInCents: 1000,
    });
  });

  it("returns failure for unknown intent", async () => {
    const args = buildFormRequest({ intent: "delete" });

    const result = await clientAction(args);

    expect(result).toEqual({ ok: false, error: { message: "unknown intent", status: 400 } });
  });
});
