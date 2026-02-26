import { describe, it, expect, afterEach } from "vitest";
import { render, cleanup } from "@testing-library/react";
import { Spinner } from "@cashregister/components/spinner";

afterEach(cleanup);

describe("Spinner", () => {
  it("renders a spinning element", () => {
    const { container } = render(<Spinner />);

    expect(container.querySelector(".animate-spin")).not.toBeNull();
  });

  it("renders an overlay that covers the parent", () => {
    const { container } = render(<Spinner />);

    const overlay = container.firstElementChild;
    expect(overlay?.classList.contains("inset-0")).toBe(true);
  });
});
