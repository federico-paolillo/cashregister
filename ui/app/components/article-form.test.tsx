import { describe, it, expect, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import { ModalIdProvider } from "./use-modal";
import { ArticleForm } from "./article-form";

afterEach(cleanup);

function renderInModal(ui: React.ReactNode) {
  return render(<ModalIdProvider id="dlg-1">{ui}</ModalIdProvider>);
}

describe("ArticleForm", () => {
  it("renders inputs with correct name attributes for FormData serialization", () => {
    renderInModal(<ArticleForm />);

    expect(screen.getByLabelText("Description")).toHaveProperty(
      "name",
      "description",
    );
    expect(screen.getByLabelText("Price (cents)")).toHaveProperty(
      "name",
      "priceInCents",
    );
  });

  it("initializes fields from initialData", () => {
    renderInModal(
      <ArticleForm
        initialData={{ description: "Latte", priceInCents: 450 }}
      />,
    );

    expect(screen.getByLabelText("Description")).toHaveProperty(
      "value",
      "Latte",
    );
    expect(screen.getByLabelText("Price (cents)")).toHaveProperty(
      "value",
      "450",
    );
  });

  it("disables both buttons when pending", () => {
    renderInModal(<ArticleForm pending={true} />);

    expect(
      screen.getByRole("button", { name: "Cancel" }),
    ).toHaveProperty("disabled", true);
    expect(screen.getByRole("button", { name: "Save" })).toHaveProperty(
      "disabled",
      true,
    );
  });

  it("sets command and commandfor on the cancel button", () => {
    renderInModal(<ArticleForm />);

    const cancel = screen.getByRole("button", { name: "Cancel" });
    expect(cancel.getAttribute("command")).toBe("close");
    expect(cancel.getAttribute("commandfor")).toBe("dlg-1");
  });
});
