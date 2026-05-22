import { z } from "zod";

const settingsSchema = z.object({
  apiBaseUrl: z.string().default("/api"),
  lowQuantityWarningThreshold: z.coerce.number().int().nonnegative().default(5),
});

export type Settings = z.infer<typeof settingsSchema>;

export function mustParseConfiguration(): Settings {
  return settingsSchema.parse({
    apiBaseUrl: import.meta.env.VITE_API_BASE_URL,
    lowQuantityWarningThreshold: import.meta.env.VITE_LOW_QUANTITY_WARNING_THRESHOLD,
  });
}
