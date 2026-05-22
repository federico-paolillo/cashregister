import { describe, it, expect, vi, afterEach } from "vitest";
import { render, screen, cleanup, fireEvent } from "@testing-library/react";
import { BulkRow } from "@cashregister/routes/articles-bulk/components/bulk-row";

afterEach(cleanup);

describe("BulkRow", () => {
  it("renders the description and price inputs", () => {
    render(<BulkRow rowId={1} onRemove={vi.fn()} canRemove={false} />);

    expect(screen.getByLabelText("Description")).toBeDefined();
    expect(screen.getByLabelText("Price")).toBeDefined();
    expect(screen.getByLabelText("Detail receipt")).toBeDefined();
    expect(screen.getByLabelText("Quantity available")).toBeDefined();
    expect(screen.getByLabelText("Available pieces")).toBeDefined();
  });

  it("does not show the Remove button when canRemove is false", () => {
    render(<BulkRow rowId={1} onRemove={vi.fn()} canRemove={false} />);

    expect(screen.queryByRole("button", { name: "Remove" })).toBeNull();
  });

  it("shows the Remove button when canRemove is true", () => {
    render(<BulkRow rowId={1} onRemove={vi.fn()} canRemove={true} />);

    expect(screen.getByRole("button", { name: "Remove" })).toBeDefined();
  });

  it("calls onRemove when the Remove button is clicked", () => {
    const onRemove = vi.fn();
    render(<BulkRow rowId={1} onRemove={onRemove} canRemove={true} />);

    fireEvent.click(screen.getByRole("button", { name: "Remove" }));

    expect(onRemove).toHaveBeenCalledOnce();
  });

  it("price input defaults to 0", () => {
    render(<BulkRow rowId={1} onRemove={vi.fn()} canRemove={false} />);

    expect(screen.getByLabelText("Price")).toHaveProperty("value", "0.00");
  });

  it("label is associated with the description input", () => {
    render(<BulkRow rowId={1} onRemove={vi.fn()} canRemove={false} />);
    expect(screen.getByLabelText("Description")).toBeDefined();
  });

  it("label is associated with the price input", () => {
    render(<BulkRow rowId={1} onRemove={vi.fn()} canRemove={false} />);
    expect(screen.getByLabelText("Price")).toBeDefined();
  });

  it("defaults detail receipts to enabled and submits the row id", () => {
    render(<BulkRow rowId={42} onRemove={vi.fn()} canRemove={false} />);

    expect(screen.getByLabelText("Detail receipt")).toHaveProperty("checked", true);
    expect(screen.getByLabelText("Detail receipt")).toHaveProperty("value", "42");
    expect(document.querySelector<HTMLInputElement>('input[name="rowId"]')?.value).toBe("42");
  });

  it("defaults quantity available to disabled", () => {
    render(<BulkRow rowId={42} onRemove={vi.fn()} canRemove={false} />);

    expect(screen.getByLabelText("Quantity available")).toHaveProperty("checked", false);
    expect(screen.getByLabelText("Available pieces")).toHaveProperty("disabled", true);

    fireEvent.click(screen.getByLabelText("Quantity available"));

    expect(screen.getByLabelText("Available pieces")).toHaveProperty("disabled", false);
    expect(screen.getByLabelText("Available pieces")).toHaveProperty("name", "quantityAvailable-42");
  });
});
