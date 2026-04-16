import { useEffect } from "react";
import { Form, useNavigation } from "react-router";
import { Spinner } from "@cashregister/components/spinner";
import { useErrorMessages } from "@cashregister/components/use-error-messages";
import { useLoaderError } from "@cashregister/components/use-loader-error";
import { deps } from "@cashregister/deps";
import type { DeviceDto } from "@cashregister/model";
import { failure } from "@cashregister/result";
import { DevicesTable } from "@cashregister/routes/devices/components/devices-table";
import type { Route } from "./+types/devices";

export async function clientLoader() {
  return deps.apiClient.get<DeviceDto[]>("/devices");
}

export async function clientAction({ request }: Route.ClientActionArgs) {
  const formData = await request.formData();
  const deviceId = formData.get("deviceId");

  if (typeof deviceId !== "string" || deviceId.length === 0) {
    return failure({ message: "missing device id", status: 400 });
  }

  return deps.apiClient.post(`/devices/${deviceId}`);
}

export default function Devices({ loaderData, actionData }: Route.ComponentProps) {
  const navigation = useNavigation();
  const { addError } = useErrorMessages();
  const devices = loaderData.ok ? loaderData.value : [];
  const isSubmitting = navigation.state === "submitting";

  useLoaderError(loaderData);

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
        <Form method="post">
          <DevicesTable devices={devices} disabled={isSubmitting} />
        </Form>
        {isSubmitting && <Spinner />}
      </main>
    </>
  );
}
