import { describe, it, expect, vi, afterEach, beforeEach } from "vitest";
import { render, screen, cleanup, act } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ErrorMessageList } from "@cashregister/components/error-message-list";
import { ErrorMessagesProvider } from "@cashregister/components/use-error-messages";
import { useErrorMessages } from "@cashregister/components/use-error-messages";

afterEach(cleanup);

function AddErrorButton({ message }: { message: string }) {
  const { addError } = useErrorMessages();
  return <button onClick={() => addError(message)}>add error</button>;
}

function renderWithProvider(
  ui: React.ReactNode,
  { autoDismissMs = 5000, maxMessages = 5 } = {},
) {
  return render(
    <ErrorMessagesProvider autoDismissMs={autoDismissMs} maxMessages={maxMessages}>
      {ui}
    </ErrorMessagesProvider>,
  );
}

describe("ErrorMessageList", () => {
  beforeEach(() => {
    vi.useFakeTimers({ shouldAdvanceTime: true });
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("renders nothing when there are no errors", () => {
    const { container } = renderWithProvider(<ErrorMessageList />);
    expect(container.innerHTML).toBe("");
  });

  it("renders an error after one is added", async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });

    renderWithProvider(
      <>
        <AddErrorButton message="Network failed" />
        <ErrorMessageList />
      </>,
    );

    await user.click(screen.getByText("add error"));

    expect(screen.getByText("Network failed")).toBeDefined();
  });

  it("renders multiple errors stacked", async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });

    function AddMultiple() {
      const { addError } = useErrorMessages();
      return (
        <button
          onClick={() => {
            addError("Error one");
            addError("Error two");
          }}
        >
          add both
        </button>
      );
    }

    renderWithProvider(
      <>
        <AddMultiple />
        <ErrorMessageList />
      </>,
    );

    await user.click(screen.getByText("add both"));

    expect(screen.getByText("Error one")).toBeDefined();
    expect(screen.getByText("Error two")).toBeDefined();
    expect(screen.getAllByRole("alert")).toHaveLength(2);
  });

  it("dismisses an error when the dismiss button is clicked", async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });

    renderWithProvider(
      <>
        <AddErrorButton message="dismiss me" />
        <ErrorMessageList />
      </>,
    );

    await user.click(screen.getByText("add error"));
    expect(screen.getByText("dismiss me")).toBeDefined();

    await user.click(screen.getByLabelText("Dismiss"));
    expect(screen.queryByText("dismiss me")).toBeNull();
  });

  it("auto-dismisses errors after the configured timeout", async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });

    renderWithProvider(
      <>
        <AddErrorButton message="vanishing" />
        <ErrorMessageList />
      </>,
      { autoDismissMs: 2000 },
    );

    await user.click(screen.getByText("add error"));
    expect(screen.getByText("vanishing")).toBeDefined();

    act(() => {
      vi.advanceTimersByTime(2000);
    });

    expect(screen.queryByText("vanishing")).toBeNull();
  });

  it("renders nothing again after all errors are dismissed", async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });

    renderWithProvider(
      <>
        <AddErrorButton message="only one" />
        <ErrorMessageList />
      </>,
    );

    await user.click(screen.getByText("add error"));
    await user.click(screen.getByLabelText("Dismiss"));

    expect(screen.queryByRole("alert")).toBeNull();
  });
});

describe("ErrorMessageList circular buffer", () => {
  beforeEach(() => {
    vi.useFakeTimers({ shouldAdvanceTime: true });
  });

  afterEach(() => {
    vi.useRealTimers();
  });

  it("evicts the oldest error when maxMessages is reached", async () => {
    const user = userEvent.setup({ advanceTimers: vi.advanceTimersByTime });

    function AddThree() {
      const { addError } = useErrorMessages();
      return (
        <button
          onClick={() => {
            addError("first");
            addError("second");
            addError("third");
          }}
        >
          add three
        </button>
      );
    }

    renderWithProvider(
      <>
        <AddThree />
        <ErrorMessageList />
      </>,
      { maxMessages: 2 },
    );

    await user.click(screen.getByText("add three"));

    expect(screen.queryByText("first")).toBeNull();
    expect(screen.getByText("second")).toBeDefined();
    expect(screen.getByText("third")).toBeDefined();
    expect(screen.getAllByRole("alert")).toHaveLength(2);
  });
});
