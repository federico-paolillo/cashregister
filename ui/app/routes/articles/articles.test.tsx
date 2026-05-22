import React from "react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup, waitFor } from "@testing-library/react";
import Articles, { clientAction, clientLoader } from "@cashregister/routes/articles/articles";
import * as reactRouter from "react-router";
import * as errorMessages from "@cashregister/components/use-error-messages";
import { deps } from "@cashregister/deps";
import type { ArticleDto, ArticlesPageDto } from "@cashregister/model";
import type { Result } from "@cashregister/result";
import type { Route } from "./+types/articles";

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof reactRouter>();
  return {
    ...actual,
    useNavigation: vi.fn(),
    useNavigate: vi.fn(),
    useFetcher: vi.fn(),
    Form: ({ children, ...props }: React.FormHTMLAttributes<HTMLFormElement>) =>
      <form {...props}>{children}</form>,
    Link: ({ children, to, ...props }: { children: React.ReactNode; to: string } & React.AnchorHTMLAttributes<HTMLAnchorElement>) =>
      <a href={String(to)} {...props}>{children}</a>,
  };
});

vi.mock("@cashregister/components/use-error-messages", () => ({
  useErrorMessages: vi.fn(),
}));

vi.mock("@cashregister/deps", () => ({
  deps: {
    apiClient: {
      get: vi.fn(),
      post: vi.fn(),
      del: vi.fn(),
    },
  },
}));

const pageData: ArticlesPageDto = {
  items: [
    { id: "1", description: "Article 1", priceInCents: 1000, quantityAvailable: null },
    { id: "2", description: "Article 2", priceInCents: 2000, quantityAvailable: null },
  ],
  next: "cursor-1",
  hasNext: true,
};

const pageResult: Result<ArticlesPageDto> = {
  ok: true,
  value: pageData,
};

const article: ArticleDto = {
  id: "1",
  description: "Article 1",
  priceInCents: 1000,
  printDetailReceipt: false,
  quantityAvailable: null,
};

const articleResult: Result<ArticleDto> = {
  ok: true,
  value: article,
};

function createLoaderData(overrides?: Partial<{
  articlesPage: Result<ArticlesPageDto>;
  selectedArticle: Result<ArticleDto> | null;
  selectedArticleId: string | null;
  until: string | null;
}>) {
  return {
    articlesPage: pageResult,
    selectedArticle: null,
    selectedArticleId: null,
    until: null,
    ...overrides,
  };
}

function renderArticles(props: Partial<Route.ComponentProps> = {}) {
  return render(
    <Articles loaderData={createLoaderData()} {...props} />,
  );
}

function buildLoaderArgs(url: string): Route.ClientLoaderArgs {
  return {
    request: new Request(url),
    params: {},
  } as Route.ClientLoaderArgs;
}

describe("Articles Page", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(reactRouter.useNavigation).mockReturnValue({
      state: "idle",
      formData: undefined,
    } as reactRouter.Navigation);
    vi.mocked(reactRouter.useFetcher).mockReturnValue({
      state: "idle",
      data: undefined,
      Form: ({ children, ...props }: React.FormHTMLAttributes<HTMLFormElement>) => <form {...props}>{children}</form>,
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

  it("does not render the detail panel when no article is selected", () => {
    renderArticles();

    expect(screen.queryByRole("link", { name: "Close article details" })).toBeNull();
  });

  it("renders the detail panel when a selected article is present", () => {
    renderArticles({
      loaderData: createLoaderData({
        selectedArticle: articleResult,
        selectedArticleId: article.id,
      }),
    });

    expect(screen.getByText("Article ID")).toBeDefined();
    expect(screen.getByDisplayValue("Article 1")).toBeDefined();
    expect(screen.getByLabelText("Detail receipt")).toHaveProperty("checked", false);
    expect(screen.getByRole("link", { name: "Close article details" })).toBeDefined();
  });

  it("shows Load More button when page.next is set", () => {
    renderArticles();

    expect(screen.getByRole("button", { name: "Load More" })).toBeDefined();
  });

  it("hides Load More button when page.next is null", () => {
    renderArticles({
      loaderData: createLoaderData({
        articlesPage: {
          ok: true,
          value: { ...pageData, next: null, hasNext: false },
        },
      }),
    });

    expect(screen.queryByRole("button", { name: "Load More" })).toBeNull();
  });

  it("preserves the selected article when submitting load more", () => {
    renderArticles({
      loaderData: createLoaderData({
        selectedArticle: articleResult,
        selectedArticleId: article.id,
      }),
    });

    const preservedSelection = Array
      .from(document.querySelectorAll<HTMLInputElement>('input[name="articleId"]'))
      .find((input) => input.closest("form")?.querySelector('input[name="until"]'));

    expect(preservedSelection).toBeDefined();
    expect(preservedSelection!.value).toBe(article.id);
    expect(preservedSelection!.getAttribute("type")).toBe("hidden");
  });

  it("preserves URL selection state when selected article loading fails", () => {
    renderArticles({
      loaderData: createLoaderData({
        selectedArticleId: "1",
        selectedArticle: {
          ok: false,
          error: { message: "Article not found", status: 404 },
        },
      }),
    });

    const selectedRow = screen.getByText("Article 1").closest("tr");
    const preservedSelection = screen.getByDisplayValue("1");

    expect(selectedRow?.className).toContain("bg-blue-100");
    expect(preservedSelection.getAttribute("name")).toBe("articleId");
    expect(preservedSelection.getAttribute("type")).toBe("hidden");
    expect(screen.queryByRole("link", { name: "Close article details" })).toBeNull();
  });

  it("shows loading state only for the load more submission", () => {
    const formData = new FormData();
    formData.set("until", "cursor-1");

    vi.mocked(reactRouter.useNavigation).mockReturnValue({
      state: "loading",
      formData,
    } as reactRouter.Navigation);

    renderArticles();

    expect(screen.getByRole("button", { name: "Loading..." })).toHaveProperty("disabled", true);
  });

  it("keeps the load more button label when loading a selection change", () => {
    const formData = new FormData();
    formData.set("articleId", "1");

    vi.mocked(reactRouter.useNavigation).mockReturnValue({
      state: "loading",
      formData,
    } as reactRouter.Navigation);

    renderArticles();

    expect(screen.getByRole("button", { name: "Load More" })).toHaveProperty("disabled", false);
  });

  it("'New Articles' link navigates without opening the create modal", () => {
    renderArticles();

    const link = screen.getByRole("link", { name: "New Articles" });

    link.click();

    expect(HTMLDialogElement.prototype.showModal).not.toHaveBeenCalled();
  });

  it("calls addError when the articles page loader returns an error", async () => {
    const addError = vi.fn();

    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError,
      dismissError: vi.fn(),
    });

    renderArticles({
      loaderData: createLoaderData({
        articlesPage: {
          ok: false,
          error: { message: "Something went wrong", status: 500 },
        },
      }),
    });

    await waitFor(() => {
      expect(addError).toHaveBeenCalledWith("Something went wrong");
    });
  });

  it("calls addError when the selected article loader returns an error", async () => {
    const addError = vi.fn();

    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError,
      dismissError: vi.fn(),
    });

    renderArticles({
      loaderData: createLoaderData({
        selectedArticleId: "missing-article",
        selectedArticle: {
          ok: false,
          error: { message: "Article not found", status: 404 },
        },
      }),
    });

    await waitFor(() => {
      expect(addError).toHaveBeenCalledWith("Article not found");
    });
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

describe("clientLoader", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("fetches only the articles page when no article is selected", async () => {
    vi.mocked(deps.apiClient.get).mockResolvedValue(pageResult);

    const result = await clientLoader(buildLoaderArgs("http://localhost/articles?until=cursor-1"));

    expect(result).toEqual({
      articlesPage: pageResult,
      selectedArticle: null,
      selectedArticleId: null,
      until: "cursor-1",
    });
    expect(deps.apiClient.get).toHaveBeenCalledTimes(1);
    expect(deps.apiClient.get).toHaveBeenCalledWith("/articles", { until: "cursor-1" });
  });

  it("fetches the articles page and the selected article when articleId is present", async () => {
    vi.mocked(deps.apiClient.get)
      .mockResolvedValueOnce(pageResult)
      .mockResolvedValueOnce(articleResult);

    const result = await clientLoader(buildLoaderArgs("http://localhost/articles?until=cursor-1&articleId=1"));

    expect(result).toEqual({
      articlesPage: pageResult,
      selectedArticle: articleResult,
      selectedArticleId: "1",
      until: "cursor-1",
    });
    expect(deps.apiClient.get).toHaveBeenNthCalledWith(1, "/articles", { until: "cursor-1" });
    expect(deps.apiClient.get).toHaveBeenNthCalledWith(2, "/articles/1");
  });
});

describe("clientAction", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("calls POST /articles/{id} for article edits", async () => {
    vi.mocked(deps.apiClient.post).mockResolvedValue({ ok: true, value: undefined });

    const args = buildFormRequest({
      articleId: "art-42",
      description: "Updated name",
      priceInCents: "500",
      printDetailReceipt: "on",
      quantityAvailableEnabled: "on",
      quantityAvailable: "-3",
    });

    await clientAction(args);

    expect(deps.apiClient.post).toHaveBeenCalledWith("/articles/art-42", {
      description: "Updated name",
      priceInCents: 500,
      printDetailReceipt: true,
      quantityAvailable: -3,
    });
  });

  it("posts disabled detail receipts for unchecked article edits", async () => {
    vi.mocked(deps.apiClient.post).mockResolvedValue({ ok: true, value: undefined });

    const args = buildFormRequest({
      articleId: "art-42",
      description: "Updated name",
      priceInCents: "500",
    });

    await clientAction(args);

    expect(deps.apiClient.post).toHaveBeenCalledWith("/articles/art-42", {
      description: "Updated name",
      priceInCents: 500,
      printDetailReceipt: false,
      quantityAvailable: null,
    });
  });

  it("returns failure when articleId is missing", async () => {
    const args = buildFormRequest({
      description: "Updated name",
      priceInCents: "500",
    });

    const result = await clientAction(args);

    expect(result).toEqual({ ok: false, error: { message: "missing article id", status: 400 } });
    expect(deps.apiClient.post).not.toHaveBeenCalled();
  });
});
