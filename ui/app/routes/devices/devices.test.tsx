import React from "react";
import { describe, it, expect, vi, beforeEach, afterEach } from "vitest";
import { render, screen, cleanup, waitFor } from "@testing-library/react";
import Devices, { clientAction, clientLoader } from "@cashregister/routes/devices/devices";
import * as reactRouter from "react-router";
import * as errorMessages from "@cashregister/components/use-error-messages";
import { deps } from "@cashregister/deps";
import type { DeviceDto } from "@cashregister/model";
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
    render(<Devices loaderData={loaderData} />);

    expect(screen.getByText("Receipt Printer A")).toBeDefined();
  });

  it("shows action errors", async () => {
    const addError = vi.fn();
    vi.mocked(errorMessages.useErrorMessages).mockReturnValue({
      errors: [],
      addError,
      dismissError: vi.fn(),
    });

    render(<Devices
      loaderData={loaderData}
      actionData={{ ok: false, error: { message: "device vanished", status: 404 } }}
    />);

    await waitFor(() => {
      expect(addError).toHaveBeenCalledWith("device vanished");
    });
  });

  it("loads devices from the API", async () => {
    vi.mocked(deps.apiClient.get).mockResolvedValueOnce(loaderData);

    const result = await clientLoader();

    expect(result).toEqual(loaderData);
    expect(deps.apiClient.get).toHaveBeenCalledWith("/devices");
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

  it("returns a failure when the device id is missing", async () => {
    const result = await clientAction({
      request: new Request("http://localhost/devices", {
        method: "POST",
        body: new FormData(),
      }),
    });

    expect(result).toEqual({
      ok: false,
      error: { message: "missing device id", status: 400 },
    });
  });
});
