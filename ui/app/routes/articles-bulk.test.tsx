import React from "react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup, fireEvent } from "@testing-library/react";
import ArticlesBulk, { clientAction } from "./articles-bulk";
import * as reactRouter from "react-router";
import { deps } from "../deps";
import type { Route } from "./+types/articles-bulk";

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof reactRouter>();
  return {
    ...actual,
    useNavigation: vi.fn(),
    Form: ({ children, ...props }: React.FormHTMLAttributes<HTMLFormElement>) =>
      <form {...props}>{children}</form>,
    Link: ({ children, to, ...props }: { children: React.ReactNode; to: string } & React.AnchorHTMLAttributes<HTMLAnchorElement>) =>
      <a href={String(to)} {...props}>{children}</a>,
  };
});

vi.mock("../deps", () => ({
  deps: {
    apiClient: {
      post: vi.fn(),
    },
  },
}));

// eslint-disable-next-line @typescript-eslint/no-explicit-any
function renderBulk(props: any = {}) {
  return render(<ArticlesBulk {...props} />);
}

describe("ArticlesBulk", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(reactRouter.useNavigation).mockReturnValue({
      state: "idle",
    } as reactRouter.Navigation);
  });

  afterEach(() => {
    cleanup();
  });

  it("renders one row on first render", () => {
    renderBulk();

    expect(screen.getAllByRole("textbox")).toHaveLength(1);
  });

  it("adds a row when + Add Article is clicked", () => {
    renderBulk();

    fireEvent.click(screen.getByRole("button", { name: "+ Add Article" }));

    expect(screen.getAllByRole("textbox")).toHaveLength(2);
  });

  it("does not show Remove button with a single row", () => {
    renderBulk();

    expect(screen.queryByRole("button", { name: "Remove" })).toBeNull();
  });

  it("shows Remove button on every row when there are multiple rows", () => {
    renderBulk();

    fireEvent.click(screen.getByRole("button", { name: "+ Add Article" }));

    expect(screen.getAllByRole("button", { name: "Remove" })).toHaveLength(2);
  });

  it("removes a row when Remove is clicked", () => {
    renderBulk();

    fireEvent.click(screen.getByRole("button", { name: "+ Add Article" }));
    fireEvent.click(screen.getAllByRole("button", { name: "Remove" })[0]);

    expect(screen.getAllByRole("textbox")).toHaveLength(1);
  });

  it("adds a row when Enter is pressed inside a text input", () => {
    renderBulk();

    fireEvent.keyDown(screen.getByRole("textbox"), { key: "Enter", bubbles: true });

    expect(screen.getAllByRole("textbox")).toHaveLength(2);
  });

  it("shows error message from actionData", () => {
    renderBulk({ actionData: { message: "2 of 3 article(s) failed to save." } });

    expect(screen.getByText("2 of 3 article(s) failed to save.")).toBeDefined();
  });

  it("disables Save when navigation is not idle", () => {
    vi.mocked(reactRouter.useNavigation).mockReturnValue({
      state: "submitting",
    } as reactRouter.Navigation);

    renderBulk();

    expect(screen.getByRole("button", { name: "Saving..." })).toHaveProperty(
      "disabled",
      true,
    );
  });
});

function buildBulkFormRequest(
  articles: Array<{ description: string; priceInCents: string }>,
): Route.ClientActionArgs {
  const formData = new FormData();
  for (const article of articles) {
    formData.append("description", article.description);
    formData.append("priceInCents", article.priceInCents);
  }
  return {
    request: new Request("http://localhost/articles/bulk", {
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

  it("calls POST /articles for each article", async () => {
    vi.mocked(deps.apiClient.post).mockResolvedValue({ ok: true, value: undefined });

    await clientAction(
      buildBulkFormRequest([
        { description: "Espresso", priceInCents: "300" },
        { description: "Latte", priceInCents: "450" },
      ]),
    );

    expect(deps.apiClient.post).toHaveBeenCalledTimes(2);
    expect(deps.apiClient.post).toHaveBeenCalledWith("/articles", {
      description: "Espresso",
      priceInCents: 300,
    });
    expect(deps.apiClient.post).toHaveBeenCalledWith("/articles", {
      description: "Latte",
      priceInCents: 450,
    });
  });

  it("redirects to /articles when all articles are saved successfully", async () => {
    vi.mocked(deps.apiClient.post).mockResolvedValue({ ok: true, value: undefined });

    const result = await clientAction(
      buildBulkFormRequest([{ description: "Espresso", priceInCents: "300" }]),
    );

    expect(result).toBeInstanceOf(Response);
    expect((result as Response).headers.get("Location")).toBe("/articles");
  });

  it("returns an error message when some articles fail to save", async () => {
    vi.mocked(deps.apiClient.post)
      .mockResolvedValueOnce({ ok: true, value: undefined })
      .mockResolvedValueOnce({ ok: false, error: { status: 400, message: "bad" } });

    const result = await clientAction(
      buildBulkFormRequest([
        { description: "Espresso", priceInCents: "300" },
        { description: "Bad article", priceInCents: "-1" },
      ]),
    );

    expect(result).toEqual({ message: "1 of 2 article(s) failed to save." });
  });
});
