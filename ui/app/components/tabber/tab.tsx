import type { ReactNode } from "react";
import {
  getTabElementId,
  getTabPanelElementId,
  useTabberContext,
} from "@cashregister/components/tabber/context";

interface TabProps {
  id: string;
  children: ReactNode;
}

export function Tab({ id, children }: TabProps) {
  const { activeTab, baseId, setActiveTab } = useTabberContext();
  const selected = activeTab === id;

  return (
    <button
      aria-controls={getTabPanelElementId(baseId, id)}
      aria-selected={selected}
      className={selected ? "tab active-tab" : "tab inactive-tab"}
      data-tab-id={id}
      id={getTabElementId(baseId, id)}
      role="tab"
      tabIndex={selected ? 0 : -1}
      type="button"
      onClick={() => setActiveTab(id)}
    >
      {children}
    </button>
  );
}
