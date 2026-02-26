import { describe, it, expect, vi, afterEach } from "vitest";
import { render, screen, cleanup, fireEvent } from "@testing-library/react";
import { ArticlesTable } from "@cashregister/routes/articles/components/articles-table";
import type { ArticleListItemDto } from "@cashregister/model";

afterEach(cleanup);

const articles: ArticleListItemDto[] = [
  { id: "1", description: "Espresso", price: 3.5 },
  { id: "2", description: "Latte", price: 4.5 },
];

describe("ArticlesTable", () => {
  it("renders all articles", () => {
    render(<ArticlesTable articles={articles} onEdit={vi.fn()} />);

    expect(screen.getByText("Espresso")).toBeDefined();
    expect(screen.getByText("Latte")).toBeDefined();
  });

  it("shows the empty message when there are no articles", () => {
    render(<ArticlesTable articles={[]} onEdit={vi.fn()} />);

    expect(screen.getByText("No articles found.")).toBeDefined();
  });

  it("does not show the empty message when there are articles", () => {
    render(<ArticlesTable articles={articles} onEdit={vi.fn()} />);

    expect(screen.queryByText("No articles found.")).toBeNull();
  });

  it("calls onEdit with the article when the edit button is clicked", () => {
    const onEdit = vi.fn();
    render(<ArticlesTable articles={articles} onEdit={onEdit} />);

    fireEvent.click(screen.getByRole("button", { name: "Edit Espresso" }));

    expect(onEdit).toHaveBeenCalledWith(articles[0]);
  });

  it("renders alternating striped rows", () => {
    render(<ArticlesTable articles={articles} onEdit={vi.fn()} />);

    const rows = screen.getAllByRole("row").slice(1); // skip header
    expect(rows[0].className).not.toContain("bg-gray-50");
    expect(rows[1].className).toContain("bg-gray-50");
  });
});
