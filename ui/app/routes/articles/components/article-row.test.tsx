import { describe, it, expect, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import { MemoryRouter } from "react-router";
import { ArticleRow } from "@cashregister/routes/articles/components/article-row";
import type { ArticleListItemDto } from "@cashregister/model";

afterEach(() => {
  cleanup();
});

const article: ArticleListItemDto = {
  id: "1",
  description: "Espresso",
  priceInCents: 350,
  printDetailReceipt: true,
  quantityAvailable: null,
};

function renderRow(props?: {
  article?: ArticleListItemDto;
  striped?: boolean;
  selected?: boolean;
  until?: string | null;
}) {
  return render(
    <MemoryRouter>
      <table>
        <tbody>
          <ArticleRow
            article={props?.article ?? article}
            striped={props?.striped ?? false}
            selected={props?.selected ?? false}
            until={props?.until ?? null}
          />
        </tbody>
      </table>
    </MemoryRouter>,
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

  it("renders a dash when quantity tracking is disabled", () => {
    renderRow();

    expect(screen.getByText("-")).toBeDefined();
  });

  it("renders the available quantity when tracking is enabled", () => {
    renderRow({ article: { ...article, quantityAvailable: 0 } });

    expect(screen.getByText("0")).toBeDefined();
  });

  it("links to the selected article on the articles route", () => {
    renderRow();

    const links = screen.getAllByRole("link");

    expect(links.length).toBeGreaterThan(0);

    for (const link of links) {
      expect(link.getAttribute("href")).toBe("/articles?articleId=1");
    }
  });

  it("preserves the pagination cursor in selection links", () => {
    renderRow({ until: "cursor-1" });

    const links = screen.getAllByRole("link");

    for (const link of links) {
      expect(link.getAttribute("href")).toBe("/articles?until=cursor-1&articleId=1");
    }
  });

  it("applies striped background when striped is true", () => {
    renderRow({ striped: true });

    const row = screen.getByText("Espresso").closest("tr");

    expect(row?.className).toContain("bg-gray-50");
  });

  it("applies selected background when selected is true", () => {
    renderRow({ striped: true, selected: true });

    const row = screen.getByText("Espresso").closest("tr");

    expect(row?.className).toContain("bg-blue-100");
    expect(row?.className).not.toContain("bg-gray-50");
  });
});
