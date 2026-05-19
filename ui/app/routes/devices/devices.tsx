import { useEffect } from "react";
import { Form, useNavigation } from "react-router";
import { Spinner } from "@cashregister/components/spinner";
import { useErrorMessages } from "@cashregister/components/use-error-messages";
import { useLoaderError } from "@cashregister/components/use-loader-error";
import { deps } from "@cashregister/deps";
import type { DeviceDto, ReceiptModeDto } from "@cashregister/model";
import { failure } from "@cashregister/result";
import { DevicesTable } from "@cashregister/routes/devices/components/devices-table";
import type { Route } from "./+types/devices";

export async function clientLoader() {
  const [devices, receiptMode] = await Promise.all([
    deps.apiClient.get<DeviceDto[]>("/devices"),
    deps.apiClient.get<ReceiptModeDto>("/receipt-mode"),
  ]);

  return { devices, receiptMode };
}

export async function clientAction({ request }: Route.ClientActionArgs) {
  const formData = await request.formData();
  const receiptMode = formData.get("receiptMode");

  if (receiptMode === "normal" || receiptMode === "detail") {
    return deps.apiClient.post(`/receipt-mode/${receiptMode}`);
  }

  const deviceId = formData.get("deviceId");

  if (typeof deviceId !== "string" || deviceId.length === 0) {
    return failure({ message: "missing device id or receipt mode", status: 400 });
  }

  return deps.apiClient.post(`/devices/${deviceId}`);
}

export default function Devices({ loaderData, actionData }: Route.ComponentProps) {
  const navigation = useNavigation();
  const { addError } = useErrorMessages();
  const devices = loaderData.devices.ok ? loaderData.devices.value : [];
  const receiptMode = loaderData.receiptMode.ok ? loaderData.receiptMode.value.mode : "normal";
  const isSubmitting = navigation.state === "submitting";

  useLoaderError(loaderData.devices);
  useLoaderError(loaderData.receiptMode);

  useEffect(() => {
    if (actionData?.ok === false) {
      addError(actionData.error.message);
    }
  }, [actionData, addError]);

  return (
    <>
      <header className="p-4 border-b">
        <h1 className="text-xl font-semibold">Devices</h1>
      </header>
      <main className="relative flex-1 overflow-auto p-4">
        <section className="mb-6">
          <h2 className="mb-3 font-semibold">Receipt Mode</h2>
          <Form method="post">
            <label className="inline-flex items-center gap-2 text-sm">
              <input
                type="hidden"
                name="receiptMode"
                value={receiptMode === "detail" ? "normal" : "detail"}
              />
              <input
                type="checkbox"
                checked={receiptMode === "detail"}
                disabled={isSubmitting}
                onChange={(e) => e.currentTarget.form?.requestSubmit()}
              />
              <span>Detail receipt mode</span>
            </label>
          </Form>
        </section>
        <Form method="post">
          <DevicesTable devices={devices} disabled={isSubmitting} />
        </Form>
        {isSubmitting && <Spinner />}
      </main>
    </>
  );
}
