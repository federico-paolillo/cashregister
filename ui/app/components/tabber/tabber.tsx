import { type ReactNode, useId, useState } from "react";
import { TabberContext } from "@cashregister/components/tabber/context";

interface TabberProps {
  defaultTab: string;
  children: ReactNode;
}

export function Tabber({ defaultTab, children }: TabberProps) {
  const [activeTab, setActiveTab] = useState(defaultTab);
  const baseId = useId();

  return (
    <TabberContext.Provider value={{ activeTab, baseId, setActiveTab }}>
      <div className="tabber">{children}</div>
    </TabberContext.Provider>
  );
}
