import { describe, it, expect, afterEach } from "vitest";
import { renderHook, act, render, screen, cleanup } from "@testing-library/react";
import { ModalIdProvider, useModalId, useModal } from "./use-modal";

afterEach(cleanup);

describe("useModalId", () => {
  it("throws when used outside a Modal", () => {
    expect(() => {
      renderHook(() => useModalId());
    }).toThrow("useModalId must be used within a <Modal>");
  });

  it("returns the provided id inside ModalIdProvider", () => {
    function Show() {
      return <span data-testid="id">{useModalId()}</span>;
    }

    render(
      <ModalIdProvider id="test-dialog-42">
        <Show />
      </ModalIdProvider>,
    );

    expect(screen.getByTestId("id").textContent).toBe("test-dialog-42");
  });
});

describe("useModal", () => {
  it("starts closed", () => {
    const { result } = renderHook(() => useModal());
    expect(result.current.isOpen).toBe(false);
  });

  it("opens and closes", () => {
    const { result } = renderHook(() => useModal());

    act(() => result.current.open());
    expect(result.current.isOpen).toBe(true);

    act(() => result.current.close());
    expect(result.current.isOpen).toBe(false);
  });
});
