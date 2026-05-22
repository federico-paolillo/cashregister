import React from "react";
import { describe, it, expect, vi, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import { ArticlesTable } from "@cashregister/routes/articles/components/articles-table";
import * as reactRouter from "react-router";
import type { ArticleListItemDto } from "@cashregister/model";

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof reactRouter>();
  return {
    ...actual,
    Link: ({ children, to, ...props }: { children: React.ReactNode; to: string } & React.AnchorHTMLAttributes<HTMLAnchorElement>) =>
      <a href={String(to)} {...props}>{children}</a>,
  };
});

afterEach(cleanup);

const articles: ArticleListItemDto[] = [
  { id: "1", description: "Espresso", priceInCents: 350, quantityAvailable: null },
  { id: "2", description: "Latte", priceInCents: 450, quantityAvailable: -2 },
];

describe("ArticlesTable", () => {
  it("renders all articles", () => {
    render(<ArticlesTable articles={articles} selectedArticleId={null} until={null} />);

    expect(screen.getByText("Espresso")).toBeDefined();
    expect(screen.getByText("Latte")).toBeDefined();
  });

  it("renders the available quantity column", () => {
    render(<ArticlesTable articles={articles} selectedArticleId={null} until={null} />);

    expect(screen.getByRole("columnheader", { name: "Available quantity" })).toBeDefined();
    expect(screen.getByText("-2")).toBeDefined();
    expect(screen.getByText("-")).toBeDefined();
  });

  it("shows the empty message when there are no articles", () => {
    render(<ArticlesTable articles={[]} selectedArticleId={null} until={null} />);

    expect(screen.getByText("No articles found.")).toHaveProperty("colSpan", 3);
  });

  it("does not render the actions column", () => {
    render(<ArticlesTable articles={articles} selectedArticleId={null} until={null} />);

    expect(screen.queryByRole("columnheader", { name: "Actions" })).toBeNull();
  });

  it("renders alternating striped rows", () => {
    render(<ArticlesTable articles={articles} selectedArticleId={null} until={null} />);

    const rows = screen.getAllByRole("row").slice(1);
    expect(rows[0].className).not.toContain("bg-gray-50");
    expect(rows[1].className).toContain("bg-gray-50");
  });

  it("marks the selected row", () => {
    render(<ArticlesTable articles={articles} selectedArticleId="2" until={null} />);

    const selectedRow = screen.getByText("Latte").closest("tr");

    expect(selectedRow?.className).toContain("bg-blue-100");
  });
});
