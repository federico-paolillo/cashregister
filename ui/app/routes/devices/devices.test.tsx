import React from "react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup, waitFor } from "@testing-library/react";
import Devices, { clientAction, clientLoader } from "@cashregister/routes/devices/devices";
import * as reactRouter from "react-router";
import * as errorMessages from "@cashregister/components/use-error-messages";
import { deps } from "@cashregister/deps";
import type { DeviceDto, ReceiptModeDto } from "@cashregister/model";
import type { Result } from "@cashregister/result";

vi.mock("react-router", async (importOriginal) => {
  const actual = await importOriginal<typeof reactRouter>();
  return {
    ...actual,
    useNavigation: vi.fn(),
    Form: ({ children, ...props }: React.FormHTMLAttributes<HTMLFormElement>) =>
      <form {...props}>{children}</form>,
  };
});

vi.mock("@cashregister/components/use-error-messages", () => ({
  useErrorMessages: vi.fn(),
}));

vi.mock("@cashregister/deps", () => ({
  deps: {
    apiClient: {
      get: vi.fn(),
      post: vi.fn(),
    },
  },
}));

const devices: DeviceDto[] = [
  {
    id: "printer-0",
    name: "Receipt Printer A",
    target: "/dev/usb/lp0",
    description: null,
    selected: true,
  },
];

const loaderData: Result<DeviceDto[]> = {
  ok: true,
  value: devices,
};

const receiptModeData: Result<ReceiptModeDto> = {
  ok: true,
  value: { mode: "normal" },
};

const routeLoaderData = {
  devices: loaderData,
  receiptMode: receiptModeData,
};

describe("Devices", () => {
  beforeEach(() => {
    vi.clearAllMocks();
    vi.mocked(reactRouter.useNavigation).mockReturnValue({
      state: "idle",
    } as reactRouter.Navigation);
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError: vi.fn(),
      dismissError: vi.fn(),
    });
  });

  afterEach(() => {
    cleanup();
  });

  it("renders devices from loader data", () => {
    render(<Devices loaderData={routeLoaderData} />);

    expect(screen.getByText("Receipt Printer A")).toBeDefined();
  });

  it("renders detail receipt mode as unchecked when mode is normal", () => {
    render(<Devices loaderData={routeLoaderData} />);

    expect(screen.getByRole("checkbox", { name: "Detail receipt mode" })).toHaveProperty("checked", false);
  });

  it("renders detail receipt mode as checked when mode is detail", () => {
    render(<Devices
      loaderData={{
        devices: loaderData,
        receiptMode: { ok: true, value: { mode: "detail" } },
      }}
    />);

    expect(screen.getByRole("checkbox", { name: "Detail receipt mode" })).toHaveProperty("checked", true);
  });

  it("shows action errors", async () => {
    const addError = vi.fn();
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError,
      dismissError: vi.fn(),
    });

    render(<Devices
      loaderData={routeLoaderData}
      actionData={{ ok: false, error: { message: "device vanished", status: 404 } }}
    />);

    await waitFor(() => {
      expect(addError).toHaveBeenCalledWith("device vanished");
    });
  });

  it("loads devices from the API", async () => {
    vi.mocked(deps.apiClient.get)
      .mockResolvedValueOnce(loaderData)
      .mockResolvedValueOnce(receiptModeData);

    const result = await clientLoader();

    expect(result).toEqual(routeLoaderData);
    expect(deps.apiClient.get).toHaveBeenCalledWith("/devices");
    expect(deps.apiClient.get).toHaveBeenCalledWith("/receipt-mode");
  });

  it("posts selected device id to the API", async () => {
    vi.mocked(deps.apiClient.post).mockResolvedValue({ ok: true, value: undefined });

    const formData = new FormData();
    formData.set("deviceId", "printer-1");

    const result = await clientAction({
      request: new Request("http://localhost/devices", {
        method: "POST",
        body: formData,
      }),
    });

    expect(result).toEqual({ ok: true, value: undefined });
    expect(deps.apiClient.post).toHaveBeenCalledWith("/devices/printer-1");
  });

  it("posts selected receipt mode to the API", async () => {
    vi.mocked(deps.apiClient.post).mockResolvedValue({ ok: true, value: undefined });

    const formData = new FormData();
    formData.set("receiptMode", "detail");

    const result = await clientAction({
      request: new Request("http://localhost/devices", {
        method: "POST",
        body: formData,
      }),
    });

    expect(result).toEqual({ ok: true, value: undefined });
    expect(deps.apiClient.post).toHaveBeenCalledWith("/receipt-mode/detail");
  });

  it("returns a failure when the device id is missing", async () => {
    const result = await clientAction({
      request: new Request("http://localhost/devices", {
        method: "POST",
        body: new FormData(),
      }),
    });

    expect(result).toEqual({
      ok: false,
      error: { message: "missing device id or receipt mode", status: 400 },
    });
  });
});
