import { describe, it, expect, vi, beforeEach } from "vitest";
import { render } from "@testing-library/react";
import { Modal } from "@cashregister/components/modal";

const showModal = vi.fn();
const close = vi.fn();

beforeEach(() => {
  showModal.mockReset();
  close.mockReset();

  HTMLDialogElement.prototype.showModal = showModal;
  HTMLDialogElement.prototype.close = close;
});

describe("Modal", () => {
  it("calls showModal when open is true", () => {
    render(
      <Modal open={true} onClose={() => { }}>
        content
      </Modal>,
    );

    expect(showModal).toHaveBeenCalledOnce();
  });

  it("calls close when open is false", () => {
    render(
      <Modal open={false} onClose={() => { }}>
        content
      </Modal>,
    );

    expect(close).toHaveBeenCalledOnce();
  });

  it("calls close when open transitions from true to false", () => {
    const { rerender } = render(
      <Modal open={true} onClose={() => { }}>
        content
      </Modal>,
    );

    rerender(
      <Modal open={false} onClose={() => { }}>
        content
      </Modal>,
    );

    expect(showModal).toHaveBeenCalledOnce();
    expect(close).toHaveBeenCalledOnce();
  });

  it("renders children regardless of open state", () => {
    const { container } = render(
      <Modal open={false} onClose={() => { }}>
        <span>always here</span>
      </Modal>,
    );

    expect(container.querySelector("span")?.textContent).toBe("always here");
  });

  it("sets closedby to none", () => {
    const { container } = render(
      <Modal open={false} onClose={() => { }}>
        content
      </Modal>,
    );

    expect(container.querySelector("dialog")?.getAttribute("closedby")).toBe(
      "none",
    );
  });

  it("sets overflow-hidden on the dialog", () => {
    const { container } = render(
      <Modal open={false} onClose={() => { }}>
        content
      </Modal>,
    );

    expect(
      container.querySelector("dialog")?.classList.contains("overflow-hidden"),
    ).toBe(true);
  });

  it("fires onClose when the dialog emits a close event", () => {
    const onClose = vi.fn();

    const { container } = render(
      <Modal open={false} onClose={onClose}>
        content
      </Modal>,
    );

    container
      .querySelector("dialog")!
      .dispatchEvent(new Event("close", { bubbles: false }));

    expect(onClose).toHaveBeenCalledOnce();
  });
});
