import { describe, it, expect, vi, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { ModalIdProvider } from "./use-modal";
import { ArticleForm } from "./article-form";

afterEach(cleanup);

function renderInModal(ui: React.ReactNode) {
  return render(<ModalIdProvider id="dlg-1">{ui}</ModalIdProvider>);
}

describe("ArticleForm", () => {
  it("calls onSave with the entered data on submit", async () => {
    const user = userEvent.setup();
    const onSave = vi.fn();

    renderInModal(<ArticleForm onSave={onSave} />);

    await user.type(screen.getByLabelText("Description"), "Espresso");
    await user.clear(screen.getByLabelText("Price (cents)"));
    await user.type(screen.getByLabelText("Price (cents)"), "350");
    await user.click(screen.getByRole("button", { name: "Save" }));

    expect(onSave).toHaveBeenCalledWith({
      description: "Espresso",
      priceInCents: 350,
    });
  });

  it("initializes fields from initialData", () => {
    renderInModal(
      <ArticleForm
        onSave={() => {}}
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
    renderInModal(<ArticleForm onSave={() => {}} pending={true} />);

    expect(
      screen.getByRole("button", { name: "Cancel" }),
    ).toHaveProperty("disabled", true);
    expect(screen.getByRole("button", { name: "Save" })).toHaveProperty(
      "disabled",
      true,
    );
  });

  it("sets command and commandfor on the cancel button", () => {
    renderInModal(<ArticleForm onSave={() => {}} />);

    const cancel = screen.getByRole("button", { name: "Cancel" });
    expect(cancel.getAttribute("command")).toBe("close");
    expect(cancel.getAttribute("commandfor")).toBe("dlg-1");
  });
});
