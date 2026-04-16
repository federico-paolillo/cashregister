import { describe, it, expect, afterEach } from "vitest";
import { render, screen, cleanup } from "@testing-library/react";
import { DevicesTable } from "@cashregister/routes/devices/components/devices-table";
import type { DeviceDto } from "@cashregister/model";

afterEach(cleanup);

const devices: DeviceDto[] = [
  {
    id: "printer-0",
    name: "Receipt Printer A",
    target: "/dev/usb/lp0",
    description: "Front counter",
    selected: true,
  },
  {
    id: "printer-1",
    name: "Receipt Printer B",
    target: "/dev/usb/lp1",
    description: null,
    selected: false,
  },
];

describe("DevicesTable", () => {
  it("renders all devices", () => {
    render(<DevicesTable devices={devices} disabled={false} />);

    expect(screen.getByText("Receipt Printer A")).toBeDefined();
    expect(screen.getByText("Receipt Printer B")).toBeDefined();
    expect(screen.getByText("/dev/usb/lp0")).toBeDefined();
  });

  it("marks the selected device and hides its select button", () => {
    render(<DevicesTable devices={devices} disabled={false} />);

    const selectedRow = screen.getByText("Receipt Printer A").closest("tr");

    expect(selectedRow?.className).toContain("bg-green-50");
    expect(selectedRow?.textContent).toContain("Selected");
    expect(selectedRow?.querySelector("button")).toBeNull();
  });

  it("shows a select button for available devices", () => {
    render(<DevicesTable devices={devices} disabled={false} />);

    const availableRow = screen.getByText("Receipt Printer B").closest("tr");
    const button = screen.getByRole("button", { name: "Select" });

    expect(availableRow?.textContent).toContain("Available");
    expect(button).toHaveProperty("value", "printer-1");
  });

  it("shows the empty message when there are no devices", () => {
    render(<DevicesTable devices={[]} disabled={false} />);

    expect(screen.getByText("No printer devices found.")).toBeDefined();
  });
});
