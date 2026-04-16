import type { DeviceDto } from "@cashregister/model";

interface DeviceRowProps {
  device: DeviceDto;
  striped: boolean;
  disabled: boolean;
}

export function DeviceRow({ device, striped, disabled }: DeviceRowProps) {
  const rowClass = device.selected
    ? "border-b bg-green-50"
    : `border-b hover:bg-blue-50 ${striped ? "bg-gray-50" : ""}`;

  return (
    <tr className={rowClass}>
      <td className="p-2">{device.name}</td>
      <td className="p-2 font-mono text-sm">{device.target}</td>
      <td className="p-2">{device.description ?? "-"}</td>
      <td className="p-2">{device.selected ? "Selected" : "Available"}</td>
      <td className="p-2 text-right">
        {!device.selected && (
          <button
            type="submit"
            name="deviceId"
            value={device.id}
            disabled={disabled}
            className="btn-primary"
          >
            Select
          </button>
        )}
      </td>
    </tr>
  );
}
