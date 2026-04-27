import React from "react";
import { describe, it, expect, vi, afterEach, beforeEach } from "vitest";
import { fireEvent, render, screen, cleanup, waitFor } from "@testing-library/react";
import { ArticleDetailPanel } from "@cashregister/routes/articles/components/article-detail-panel";
import * as reactRouter from "react-router";
import * as errorMessages from "@cashregister/components/use-error-messages";
import { deps } from "@cashregister/deps";
import type { ArticleDto } from "@cashregister/model";

const navigate = vi.fn();

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof reactRouter>();
  return {
    ...actual,
    useFetcher: vi.fn(),
    useNavigate: () => navigate,
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
      del: vi.fn(),
    },
  },
}));

const article: ArticleDto = {
  id: "art-1",
  description: "Espresso",
  priceInCents: 350,
};

describe("ArticleDetailPanel", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    navigate.mockClear();
    vi.mocked(reactRouter.useFetcher).mockReturnValue({
      state: "idle",
      data: undefined,
      formData: undefined,
      Form: ({ children, ...props }: React.FormHTMLAttributes<HTMLFormElement>) => <form {...props}>{children}</form>,
    } as unknown as reactRouter.FetcherWithComponents<unknown>);
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError: vi.fn(),
      dismissError: vi.fn(),
    });
  });

  afterEach(cleanup);

  it("renders article metadata and close link", () => {
    render(<ArticleDetailPanel article={article} closeTo="/articles?until=cursor-1" />);

    expect(screen.getByText("Espresso")).toBeDefined();
    expect(screen.getByText("Article ID")).toBeDefined();
    expect(screen.getByText("art-1")).toBeDefined();
    expect(screen.getByRole("link", { name: "Close article details" }).getAttribute("href")).toBe("/articles?until=cursor-1");
  });

  it("renders the edit form initialized from the article", () => {
    render(<ArticleDetailPanel article={article} closeTo="/articles" />);

    expect(screen.getByDisplayValue("Espresso")).toBeDefined();
    expect(screen.getByDisplayValue("3.50")).toBeDefined();

    const articleId = document.querySelector<HTMLInputElement>('input[name="articleId"]');

    expect(document.querySelector<HTMLInputElement>('input[name="intent"]')).toBeNull();
    expect(articleId?.value).toBe("art-1");
    expect(screen.getByRole("button", { name: "Delete" })).toBeDefined();
    expect(screen.queryByRole("link", { name: "Cancel" })).toBeNull();
  });

  it("updates the edit form when the selected article changes", () => {
    const { rerender } = render(<ArticleDetailPanel article={article} closeTo="/articles" />);

    rerender(
      <ArticleDetailPanel
        article={{
          id: "art-2",
          description: "Latte",
          priceInCents: 450,
        }}
        closeTo="/articles"
      />,
    );

    expect(screen.getByDisplayValue("Latte")).toBeDefined();
    expect(screen.getByDisplayValue("4.50")).toBeDefined();

    const articleId = document.querySelector<HTMLInputElement>('input[name="articleId"]');

    expect(articleId?.value).toBe("art-2");
  });

  it("deletes the article and closes the panel after success", async () => {
    vi.mocked(deps.apiClient.del).mockResolvedValue({ ok: true, value: undefined });

    render(<ArticleDetailPanel article={article} closeTo="/articles?until=cursor-1" />);

    fireEvent.click(screen.getByRole("button", { name: "Delete" }));

    await waitFor(() => {
      expect(deps.apiClient.del).toHaveBeenCalledWith("/articles/art-1");
      expect(navigate).toHaveBeenCalledWith("/articles?until=cursor-1");
    });
  });

  it("reports delete failures without closing the panel", async () => {
    const addError = vi.fn();
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError,
      dismissError: vi.fn(),
    });
    vi.mocked(deps.apiClient.del).mockResolvedValue({
      ok: false,
      error: { message: "Cannot delete article", status: 400 },
    });

    render(<ArticleDetailPanel article={article} closeTo="/articles" />);

    fireEvent.click(screen.getByRole("button", { name: "Delete" }));

    await waitFor(() => {
      expect(addError).toHaveBeenCalledWith("Cannot delete article");
    });
    expect(navigate).not.toHaveBeenCalled();
  });

  it("disables the delete button while deletion is pending", async () => {
    let resolveDelete: (value: { ok: true; value: undefined }) => void = () => {};
    vi.mocked(deps.apiClient.del).mockReturnValue(
      new Promise((resolve) => {
        resolveDelete = resolve;
      }),
    );

    render(<ArticleDetailPanel article={article} closeTo="/articles" />);

    fireEvent.click(screen.getByRole("button", { name: "Delete" }));

    expect(screen.getByRole("button", { name: "Deleting..." })).toHaveProperty("disabled", true);

    resolveDelete({ ok: true, value: undefined });

    await waitFor(() => {
      expect(navigate).toHaveBeenCalledWith("/articles");
    });
  });
});
