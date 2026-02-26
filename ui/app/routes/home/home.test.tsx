import { describe, it, expect, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import Home from "@cashregister/routes/home/home";

afterEach(cleanup);

describe("Home", () => {
  it("renders the 'It works!' message", () => {
    render(<Home />);

    expect(screen.getByText("It works!")).toBeDefined();
  });
});
