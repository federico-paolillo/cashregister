import { describe, it, expect, vi, afterEach, beforeEach } from "vitest";
import {
  renderHook,
  act,
  render,
  screen,
  cleanup,
} from "@testing-library/react";
import {
  useErrorMessages,
  useErrorMessagesState,
  ErrorMessagesProvider,
} from "./use-error-messages";

afterEach(cleanup);

describe("useErrorMessages context", () => {
  it("throws when used outside a provider", () => {
    expect(() => {
      renderHook(() => useErrorMessages());
    }).toThrow("useErrorMessages must be used within an <ErrorMessagesProvider>");
  });

  it("returns context value inside provider", () => {
    const { result } = renderHook(() => useErrorMessages(), {
      wrapper: ({ children }) => (
        <ErrorMessagesProvider>{children}</ErrorMessagesProvider>
      ),
    });

    expect(result.current.errors).toEqual([]);
    expect(typeof result.current.addError).toBe("function");
    expect(typeof result.current.dismissError).toBe("function");
  });
});

describe("useErrorMessagesState", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("starts with an empty error list", () => {
    const { result } = renderHook(() => useErrorMessagesState(5000));
    expect(result.current.errors).toEqual([]);
  });

  it("adds an error", () => {
    const { result } = renderHook(() => useErrorMessagesState(5000));

    act(() => {
      result.current.addError("something broke");
    });

    expect(result.current.errors).toHaveLength(1);
    expect(result.current.errors[0].message).toBe("something broke");
  });

  it("assigns unique ids to each error", () => {
    const { result } = renderHook(() => useErrorMessagesState(5000));

    act(() => {
      result.current.addError("first");
      result.current.addError("second");
    });

    const ids = result.current.errors.map((e) => e.id);
    expect(new Set(ids).size).toBe(2);
  });

  it("stacks multiple errors in order", () => {
    const { result } = renderHook(() => useErrorMessagesState(5000));

    act(() => {
      result.current.addError("first");
      result.current.addError("second");
      result.current.addError("third");
    });

    expect(result.current.errors.map((e) => e.message)).toEqual([
      "first",
      "second",
      "third",
    ]);
  });

  it("dismisses an error by id", () => {
    const { result } = renderHook(() => useErrorMessagesState(5000));

    act(() => {
      result.current.addError("keep");
      result.current.addError("remove");
    });

    const idToRemove = result.current.errors[1].id;

    act(() => {
      result.current.dismissError(idToRemove);
    });

    expect(result.current.errors).toHaveLength(1);
    expect(result.current.errors[0].message).toBe("keep");
  });

  it("auto-dismisses after the configured timeout", () => {
    const { result } = renderHook(() => useErrorMessagesState(3000));

    act(() => {
      result.current.addError("will vanish");
    });

    expect(result.current.errors).toHaveLength(1);

    act(() => {
      vi.advanceTimersByTime(3000);
    });

    expect(result.current.errors).toHaveLength(0);
  });

  it("does not auto-dismiss before the timeout elapses", () => {
    const { result } = renderHook(() => useErrorMessagesState(5000));

    act(() => {
      result.current.addError("still here");
    });

    act(() => {
      vi.advanceTimersByTime(4999);
    });

    expect(result.current.errors).toHaveLength(1);
  });

  it("auto-dismisses errors independently", () => {
    const { result } = renderHook(() => useErrorMessagesState(3000));

    act(() => {
      result.current.addError("first");
    });

    act(() => {
      vi.advanceTimersByTime(1500);
    });

    act(() => {
      result.current.addError("second");
    });

    // At 3000ms total, first should be dismissed but second should remain
    act(() => {
      vi.advanceTimersByTime(1500);
    });

    expect(result.current.errors).toHaveLength(1);
    expect(result.current.errors[0].message).toBe("second");

    // At 4500ms total, second should also be dismissed
    act(() => {
      vi.advanceTimersByTime(1500);
    });

    expect(result.current.errors).toHaveLength(0);
  });

  it("cancels the auto-dismiss timer on manual dismiss", () => {
    const { result } = renderHook(() => useErrorMessagesState(5000));

    act(() => {
      result.current.addError("dismissed early");
    });

    const id = result.current.errors[0].id;

    act(() => {
      result.current.dismissError(id);
    });

    expect(result.current.errors).toHaveLength(0);

    // Advancing past the original timeout should not cause errors
    act(() => {
      vi.advanceTimersByTime(5000);
    });

    expect(result.current.errors).toHaveLength(0);
  });

  it("does not auto-dismiss when autoDismissMs is 0", () => {
    const { result } = renderHook(() => useErrorMessagesState(0));

    act(() => {
      result.current.addError("stays forever");
    });

    act(() => {
      vi.advanceTimersByTime(60000);
    });

    expect(result.current.errors).toHaveLength(1);
  });

  it("clears all timers on unmount", () => {
    const clearTimeoutSpy = vi.spyOn(globalThis, "clearTimeout");

    const { result, unmount } = renderHook(() => useErrorMessagesState(5000));

    act(() => {
      result.current.addError("one");
      result.current.addError("two");
    });

    const callsBefore = clearTimeoutSpy.mock.calls.length;

    unmount();

    expect(clearTimeoutSpy.mock.calls.length).toBeGreaterThan(callsBefore);

    clearTimeoutSpy.mockRestore();
  });
});

describe("ErrorMessagesProvider", () => {
  beforeEach(() => {
    vi.useFakeTimers();
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("uses the provided autoDismissMs", () => {
    function TestConsumer() {
      const { errors, addError } = useErrorMessages();
      return (
        <div>
          <button onClick={() => addError("test")}>add</button>
          <span data-testid="count">{errors.length}</span>
        </div>
      );
    }

    render(
      <ErrorMessagesProvider autoDismissMs={2000}>
        <TestConsumer />
      </ErrorMessagesProvider>,
    );

    act(() => {
      screen.getByText("add").click();
    });

    expect(screen.getByTestId("count").textContent).toBe("1");

    act(() => {
      vi.advanceTimersByTime(2000);
    });

    expect(screen.getByTestId("count").textContent).toBe("0");
  });
});
