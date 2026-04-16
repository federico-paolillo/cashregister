import type { DeviceDto } from "@cashregister/model";
import { DeviceRow } from "@cashregister/routes/devices/components/device-row";

interface DevicesTableProps {
  devices: DeviceDto[];
  disabled: boolean;
}

export function DevicesTable({ devices, disabled }: DevicesTableProps) {
  return (
    <table className="w-full border-collapse">
      <thead>
        <tr className="border-b bg-gray-100 text-left">
          <th className="p-2 font-semibold">Name</th>
          <th className="p-2 font-semibold">Target</th>
          <th className="p-2 font-semibold">Details</th>
          <th className="p-2 font-semibold">Status</th>
          <th className="p-2 font-semibold text-right">Action</th>
        </tr>
      </thead>
      <tbody>
        {devices.map((device, index) => (
          <DeviceRow
            key={device.id}
            device={device}
            striped={index % 2 === 1}
            disabled={disabled}
          />
        ))}
        {devices.length === 0 && (
          <tr>
            <td colSpan={5} className="p-4 text-center text-gray-500 text-sm italic">
              No printer devices found.
            </td>
          </tr>
        )}
      </tbody>
    </table>
  );
}
