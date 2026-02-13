import { describe, it, expect, vi, afterEach } from "vitest";
import { render, screen, cleanup, fireEvent } from "@testing-library/react";
import { ArticleRow } from "./article-row";
import type { ArticleListItemDto } from "../model";

afterEach(() => {
  cleanup();
});

const article: ArticleListItemDto = {
  id: "1",
  description: "Espresso",
  price: 3.5,
};

function renderRow(props?: { striped?: boolean; onEdit?: (a: ArticleListItemDto) => void }) {
  return render(
    <table>
      <tbody>
        <ArticleRow
          article={article}
          striped={props?.striped ?? false}
          onEdit={props?.onEdit ?? vi.fn()}
        />
      </tbody>
    </table>,
  );
}

describe("ArticleRow", () => {
  it("renders the article description", () => {
    renderRow();
    expect(screen.getByText("Espresso")).toBeDefined();
  });

  it("renders the formatted price", () => {
    renderRow();
    expect(screen.getByText("3.50")).toBeDefined();
  });

  it("renders edit and delete buttons", () => {
    renderRow();
    expect(screen.getByRole("button", { name: "Edit Espresso" })).toBeDefined();
    expect(screen.getByRole("button", { name: "Delete Espresso" })).toBeDefined();
  });

  it("calls onEdit with the article when clicking edit", () => {
    const onEdit = vi.fn();
    renderRow({ onEdit });

    fireEvent.click(screen.getByRole("button", { name: "Edit Espresso" }));

    expect(onEdit).toHaveBeenCalledWith(article);
  });

  it("has delete button disabled", () => {
    renderRow();
    expect(screen.getByRole("button", { name: "Delete Espresso" })).toHaveProperty("disabled", true);
  });

  it("applies striped background when striped is true", () => {
    renderRow({ striped: true });
    const row = screen.getByText("Espresso").closest("tr");
    expect(row?.className).toContain("bg-gray-50");
  });

  it("does not apply striped background when striped is false", () => {
    renderRow({ striped: false });
    const row = screen.getByText("Espresso").closest("tr");
    expect(row?.className).not.toContain("bg-gray-50");
  });
});
