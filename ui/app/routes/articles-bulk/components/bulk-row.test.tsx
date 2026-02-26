import { describe, it, expect, vi, afterEach } from "vitest";
import { render, screen, cleanup, fireEvent } from "@testing-library/react";
import { BulkRow } from "@cashregister/routes/articles-bulk/components/bulk-row";

afterEach(cleanup);

describe("BulkRow", () => {
  it("renders the description and price inputs", () => {
    render(<BulkRow onRemove={vi.fn()} canRemove={false} />);

    expect(screen.getByRole("textbox")).toBeDefined();
    expect(screen.getByRole("spinbutton")).toBeDefined();
  });

  it("does not show the Remove button when canRemove is false", () => {
    render(<BulkRow onRemove={vi.fn()} canRemove={false} />);

    expect(screen.queryByRole("button", { name: "Remove" })).toBeNull();
  });

  it("shows the Remove button when canRemove is true", () => {
    render(<BulkRow onRemove={vi.fn()} canRemove={true} />);

    expect(screen.getByRole("button", { name: "Remove" })).toBeDefined();
  });

  it("calls onRemove when the Remove button is clicked", () => {
    const onRemove = vi.fn();
    render(<BulkRow onRemove={onRemove} canRemove={true} />);

    fireEvent.click(screen.getByRole("button", { name: "Remove" }));

    expect(onRemove).toHaveBeenCalledOnce();
  });

  it("price input defaults to 0", () => {
    render(<BulkRow onRemove={vi.fn()} canRemove={false} />);

    expect(screen.getByRole("spinbutton")).toHaveProperty("value", "0");
  });
});
