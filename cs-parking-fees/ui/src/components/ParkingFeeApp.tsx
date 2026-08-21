// This file is part of Parking Fee Control mod.
// Copyright (C) 2026 thiago-rcarvalho
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
//
// SPDX-License-Identifier: GPL-3.0-or-later

import React, { useState, useCallback } from 'react';
import { ToolbarButton } from './ToolbarButton';
import { ParkingFeePanel } from './ParkingFeePanel';
import { InputActionConsumer } from 'cs2/input';

export const ParkingFeeApp: React.FC = () => {
  const [showPanel, setShowPanel] = useState(false);

  const handleToggle = useCallback(() => {
    setShowPanel((prev) => !prev);
  }, []);

  return (
    <>
      <ToolbarButton onClick={handleToggle} />
      {showPanel && (
        <InputActionConsumer
          ignoreFocusState
          actions={{
            Back: handleToggle,
            "Pause Menu": handleToggle,
          }}
        >
          <ParkingFeePanel onClose={handleToggle} />
        </InputActionConsumer>
      )}
    </>
  );
};
