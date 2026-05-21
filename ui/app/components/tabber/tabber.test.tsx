import { cleanup, render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import {
  Tab,
  Tabber,
  TabList,
  TabPanel,
} from "@cashregister/components/tabber";
import { afterEach, describe, expect, it } from "vitest";

afterEach(cleanup);

function renderTabber() {
  render(
    <Tabber defaultTab="articles">
      <TabList aria-label="Statistics views">
        <Tab id="articles">Articles</Tab>
        <Tab id="orders">Orders</Tab>
      </TabList>
      <TabPanel tabId="articles">Article inventory</TabPanel>
      <TabPanel tabId="orders">Order volume</TabPanel>
    </Tabber>,
  );
}

describe("Tabber", () => {
  it("selects the default tab and links tabs to their panels", () => {
    renderTabber();

    const articleTab = screen.getByRole("tab", { name: "Articles" });
    const orderTab = screen.getByRole("tab", { name: "Orders" });
    const articlePanel = screen.getByRole("tabpanel", { name: "Articles" });
    const orderPanel = screen.getByText("Order volume").closest("section");

    expect(articleTab.getAttribute("aria-selected")).toBe("true");
    expect(articleTab).toHaveProperty("tabIndex", 0);
    expect(orderTab.getAttribute("aria-selected")).toBe("false");
    expect(orderTab).toHaveProperty("tabIndex", -1);
    expect(articleTab.getAttribute("aria-controls")).toBe(articlePanel.id);
    expect(articlePanel.getAttribute("aria-labelledby")).toBe(articleTab.id);
    expect(orderPanel).toHaveProperty("hidden", true);
  });

  it("selects tabs by click", async () => {
    const user = userEvent.setup();
    renderTabber();

    await user.click(screen.getByRole("tab", { name: "Orders" }));

    expect(
      screen.getByRole("tab", { name: "Orders" }).getAttribute(
        "aria-selected",
      ),
    ).toBe("true");
    expect(screen.getByRole("tabpanel", { name: "Orders" })).toBeDefined();
    expect(screen.getByText("Article inventory").closest("section")).toHaveProperty(
      "hidden",
      true,
    );
  });

  it("selects tabs with tablist keyboard navigation", async () => {
    const user = userEvent.setup();
    renderTabber();

    const articleTab = screen.getByRole("tab", { name: "Articles" });
    const orderTab = screen.getByRole("tab", { name: "Orders" });

    articleTab.focus();

    await user.keyboard("{ArrowRight}");

    expect(document.activeElement).toBe(orderTab);
    expect(orderTab.getAttribute("aria-selected")).toBe("true");

    await user.keyboard("{Home}");

    expect(document.activeElement).toBe(articleTab);
    expect(articleTab.getAttribute("aria-selected")).toBe("true");

    await user.keyboard("{End}");

    expect(document.activeElement).toBe(orderTab);
    expect(orderTab.getAttribute("aria-selected")).toBe("true");

    await user.keyboard("{ArrowLeft}");

    expect(document.activeElement).toBe(articleTab);
    expect(articleTab.getAttribute("aria-selected")).toBe("true");
  });
});
