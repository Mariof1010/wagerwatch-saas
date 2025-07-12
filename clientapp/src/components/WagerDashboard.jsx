import React, { useState, useEffect } from "react";
import {
  DollarSign,
  Eye,
  Plus,
  Filter,
  Calendar,
  BarChart3,
  Settings,
  Bell,
  Upload,
  Download,
} from "lucide-react";

const WagerDashboard = () => {
  const [selectedTimeframe, setSelectedTimeframe] = useState("active");
  const [sidebarCollapsed, setSidebarCollapsed] = useState(false);

  // Sample data for now
  const sampleWagers = [
    {
      id: 1,
      homeTeam: "Kansas City Chiefs",
      awayTeam: "Buffalo Bills",
      gameTime: "Tonight 8:00 PM",
      betType: "Moneyline",
      amount: 100,
      odds: -110,
      potentialPayout: 190.91,
      status: "Active",
      auraColor: "yellow",
      auraSize: "medium",
      shouldPulse: true,
      selection: "Chiefs",
      description: "Chiefs to win outright",
      trackingOnly: false,
    },
    {
      id: 2,
      homeTeam: "Lakers",
      awayTeam: "Celtics",
      gameTime: "Tonight 10:30 PM",
      betType: "Point Spread",
      amount: 0,
      odds: 110,
      potentialPayout: 0,
      status: "Active",
      auraColor: "green",
      auraSize: "large",
      shouldPulse: false,
      selection: "Lakers +5.5",
      description: "Lakers cover spread (tracking only)",
      trackingOnly: true,
    },
  ];

  const timeframeData = {
    active: {
      totalWagered: 225,
      totalWon: 415,
      winRate: 67,
      roi: 84,
      betCount: 2,
      color: "yellow",
      description: "Active wagers in play",
    },
    today: {
      totalWagered: 450,
      totalWon: 521,
      winRate: 75,
      roi: 16,
      betCount: 4,
      color: "green",
      description: "All wagers placed today",
    },
  };

  const WagerIndicator = ({ wager }) => {
    const colorStyle = {
      color:
        wager.auraColor === "green"
          ? "#10B981"
          : wager.auraColor === "yellow"
          ? "#F59E0B"
          : "#EF4444",
    };

    const size =
      wager.auraSize === "large"
        ? "24px"
        : wager.auraSize === "small"
        ? "16px"
        : "20px";

    if (wager.trackingOnly) {
      return <span style={{ ...colorStyle, fontSize: size }}>🕶️</span>;
    }

    return <DollarSign style={{ ...colorStyle, width: size, height: size }} />;
  };

  const WagerWatchLogo = () => (
    <div style={{ display: "flex", alignItems: "center" }}>
      <img
        src="/images/logo.png"
        alt="WagerWatch Logo"
        style={{
          width: "48px",
          height: "48px",
          objectFit: "contain",
          background: "transparent",
        }}
        onError={(e) => {
          console.log("Logo failed to load:", e);
        }}
      />
    </div>
  );

  const containerStyle = {
    backgroundColor: "#111827",
    minHeight: "100vh",
    color: "white",
    fontFamily: "Arial, sans-serif",
  };

  const headerStyle = {
    backgroundColor: "#1F2937",
    borderBottom: "1px solid #374151",
    padding: "1rem 1.5rem",
    display: "flex",
    justifyContent: "space-between",
    alignItems: "center",
  };

  const sidebarStyle = {
    width: sidebarCollapsed ? "0" : "320px",
    backgroundColor: "#1F2937",
    minHeight: "100vh",
    borderRight: "1px solid #374151",
    transition: "width 0.3s",
    overflow: "hidden",
  };

  const mainContentStyle = {
    flex: 1,
    padding: "1.5rem",
  };

  const scopeButtonStyle = (timeframe) => ({
    backgroundColor: "#1F2937",
    border: `2px solid ${
      selectedTimeframe === timeframe ? "#3B82F6" : "#4B5563"
    }`,
    borderRadius: "12px",
    padding: "1rem 1.5rem",
    minWidth: "140px",
    cursor: "pointer",
    transition: "all 0.2s",
    marginRight: "1rem",
  });

  const statsBoxStyle = {
    border: `2px solid ${selectedTimeframe ? "#3B82F6" : "#4B5563"}`,
    borderRadius: "12px",
    backgroundColor: "#1F2937",
    padding: "1.5rem",
    marginBottom: "2rem",
    boxShadow: selectedTimeframe
      ? "0 10px 25px rgba(59, 130, 246, 0.2)"
      : "none",
  };

  const wagerCardStyle = (wager) => ({
    backgroundColor: "#1F2937",
    border: `3px solid ${
      wager.auraColor === "green"
        ? "#10B981"
        : wager.auraColor === "yellow"
        ? "#F59E0B"
        : "#EF4444"
    }`,
    borderRadius: "12px",
    padding: "1.5rem",
    marginBottom: "1rem",
    boxShadow: `0 10px 25px rgba(${
      wager.auraColor === "green"
        ? "16, 185, 129"
        : wager.auraColor === "yellow"
        ? "245, 158, 11"
        : "239, 68, 68"
    }, 0.2)`,
  });

  return (
    <div style={containerStyle}>
      {/* Header */}
      <div style={headerStyle}>
        <div style={{ display: "flex", alignItems: "center", gap: "1rem" }}>
          <button
            onClick={() => setSidebarCollapsed(!sidebarCollapsed)}
            style={{
              padding: "0.5rem",
              backgroundColor: "transparent",
              border: "none",
              color: "#9CA3AF",
              cursor: "pointer",
              borderRadius: "8px",
            }}
          >
            {sidebarCollapsed ? "☰" : "✕"}
          </button>
          <WagerWatchLogo />
        </div>
        <div style={{ display: "flex", alignItems: "center", gap: "1rem" }}>
          <Bell style={{ width: "20px", height: "20px", cursor: "pointer" }} />
          <Settings
            style={{ width: "20px", height: "20px", cursor: "pointer" }}
          />
        </div>
      </div>

      <div style={{ display: "flex" }}>
        {/* Sidebar */}
        <div style={sidebarStyle}>
          <div style={{ padding: "1rem" }}>
            {/* New Wager Button */}
            <button
              style={{
                width: "100%",
                backgroundColor: "#10B981",
                border: "none",
                borderRadius: "8px",
                padding: "0.75rem 1rem",
                color: "white",
                fontWeight: "bold",
                fontSize: "1.125rem",
                cursor: "pointer",
                display: "flex",
                alignItems: "center",
                justifyContent: "center",
                gap: "0.75rem",
                marginBottom: "1.5rem",
              }}
            >
              <Plus style={{ width: "32px", height: "32px" }} />
              Wager
            </button>

            {/* User Info */}
            <div
              style={{
                backgroundColor: "#374151",
                borderRadius: "8px",
                padding: "1rem",
                marginBottom: "1.5rem",
              }}
            >
              <div
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: "0.75rem",
                }}
              >
                <div
                  style={{
                    width: "40px",
                    height: "40px",
                    backgroundColor: "#10B981",
                    borderRadius: "50%",
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "center",
                    fontWeight: "bold",
                  }}
                >
                  M
                </div>
                <div>
                  <div style={{ fontWeight: "600" }}>Mario F.</div>
                  <div style={{ fontSize: "0.875rem", color: "#9CA3AF" }}>
                    Premium Member
                  </div>
                </div>
              </div>
            </div>

            {/* Navigation */}
            <nav>
              <a
                href="#"
                style={{
                  display: "flex",
                  alignItems: "center",
                  gap: "0.75rem",
                  padding: "0.75rem",
                  borderRadius: "8px",
                  backgroundColor: "#374151",
                  color: "#10B981",
                  textDecoration: "none",
                  marginBottom: "0.5rem",
                }}
              >
                <Eye style={{ width: "20px", height: "20px" }} />
                Dashboard
              </a>
            </nav>
          </div>
        </div>

        {/* Main Content */}
        <div style={mainContentStyle}>
          {/* Aura Scope Buttons */}
          <div
            style={{
              marginBottom: "2rem",
              display: "flex",
              justifyContent: "center",
            }}
          >
            {Object.entries(timeframeData).map(([timeframe, data]) => (
              <button
                key={timeframe}
                onClick={() => setSelectedTimeframe(timeframe)}
                style={scopeButtonStyle(timeframe)}
              >
                <div
                  style={{
                    display: "flex",
                    alignItems: "center",
                    gap: "0.5rem",
                    marginBottom: "0.5rem",
                  }}
                >
                  <DollarSign
                    style={{
                      width: "24px",
                      height: "24px",
                      color: data.color === "green" ? "#10B981" : "#F59E0B",
                    }}
                  />
                  <span
                    style={{
                      fontWeight: "bold",
                      fontSize: "1.125rem",
                      color: data.color === "green" ? "#10B981" : "#F59E0B",
                      textTransform: "capitalize",
                    }}
                  >
                    {timeframe}
                  </span>
                </div>
                <div style={{ fontSize: "0.875rem", color: "#9CA3AF" }}>
                  {data.betCount} wager{data.betCount !== 1 ? "s" : ""}
                </div>
              </button>
            ))}
          </div>

          {/* Stats Box */}
          <div style={statsBoxStyle}>
            <div
              style={{
                display: "grid",
                gridTemplateColumns: "repeat(4, 1fr)",
                gap: "1.5rem",
              }}
            >
              <div style={{ textAlign: "center" }}>
                <div
                  style={{
                    color: "#10B981",
                    fontWeight: "600",
                    fontSize: "0.875rem",
                    marginBottom: "0.5rem",
                  }}
                >
                  TOTAL WAGERED
                </div>
                <div style={{ fontSize: "1.875rem", fontWeight: "bold" }}>
                  ${timeframeData[selectedTimeframe].totalWagered}
                </div>
              </div>
              <div style={{ textAlign: "center" }}>
                <div
                  style={{
                    color: "#10B981",
                    fontWeight: "600",
                    fontSize: "0.875rem",
                    marginBottom: "0.5rem",
                  }}
                >
                  {selectedTimeframe === "active"
                    ? "POTENTIAL WIN"
                    : "TOTAL WON"}
                </div>
                <div style={{ fontSize: "1.875rem", fontWeight: "bold" }}>
                  ${timeframeData[selectedTimeframe].totalWon}
                </div>
              </div>
              <div style={{ textAlign: "center" }}>
                <div
                  style={{
                    color: "#10B981",
                    fontWeight: "600",
                    fontSize: "0.875rem",
                    marginBottom: "0.5rem",
                  }}
                >
                  WIN RATE
                </div>
                <div style={{ fontSize: "1.875rem", fontWeight: "bold" }}>
                  {timeframeData[selectedTimeframe].winRate}%
                </div>
              </div>
              <div style={{ textAlign: "center" }}>
                <div
                  style={{
                    color: "#10B981",
                    fontWeight: "600",
                    fontSize: "0.875rem",
                    marginBottom: "0.5rem",
                  }}
                >
                  ROI
                </div>
                <div
                  style={{
                    fontSize: "1.875rem",
                    fontWeight: "bold",
                    color:
                      timeframeData[selectedTimeframe].roi >= 0
                        ? "#10B981"
                        : "#EF4444",
                  }}
                >
                  {timeframeData[selectedTimeframe].roi > 0 ? "+" : ""}
                  {timeframeData[selectedTimeframe].roi}%
                </div>
              </div>
            </div>
            <div style={{ marginTop: "1rem", textAlign: "center" }}>
              <div style={{ color: "#9CA3AF", fontSize: "0.875rem" }}>
                {timeframeData[selectedTimeframe].description}
              </div>
            </div>
          </div>

          {/* Wager List */}
          <div style={{ marginBottom: "1.5rem" }}>
            <h3
              style={{
                fontSize: "1.25rem",
                fontWeight: "bold",
                display: "flex",
                alignItems: "center",
                gap: "0.75rem",
                marginBottom: "1.5rem",
              }}
            >
              <DollarSign
                style={{
                  width: "32px",
                  height: "32px",
                  color:
                    timeframeData[selectedTimeframe].color === "green"
                      ? "#10B981"
                      : "#F59E0B",
                }}
              />
              {selectedTimeframe === "active"
                ? "Active Wagers"
                : "Today's Wagers"}
              <span style={{ color: "#9CA3AF", fontWeight: "normal" }}>
                ({sampleWagers.length})
              </span>
            </h3>
          </div>

          {/* Wager Cards */}
          <div>
            {sampleWagers.map((wager) => (
              <div key={wager.id} style={wagerCardStyle(wager)}>
                <div
                  style={{
                    display: "flex",
                    alignItems: "center",
                    justifyContent: "space-between",
                  }}
                >
                  <div
                    style={{
                      display: "flex",
                      alignItems: "center",
                      gap: "1.5rem",
                    }}
                  >
                    <div
                      style={{
                        display: "flex",
                        flexDirection: "column",
                        alignItems: "center",
                      }}
                    >
                      <WagerIndicator wager={wager} />
                      <div
                        style={{
                          fontSize: "0.75rem",
                          color: "#9CA3AF",
                          marginTop: "0.25rem",
                          fontWeight: "500",
                        }}
                      >
                        {wager.auraSize.toUpperCase()}
                      </div>
                      {wager.trackingOnly && (
                        <div
                          style={{
                            fontSize: "0.75rem",
                            marginTop: "0.25rem",
                            padding: "0.25rem 0.5rem",
                            borderRadius: "4px",
                            backgroundColor: "rgba(59, 130, 246, 0.2)",
                            color: "#60A5FA",
                          }}
                        >
                          TRACKING
                        </div>
                      )}
                    </div>
                    <div style={{ flex: 1 }}>
                      <div
                        style={{
                          fontWeight: "bold",
                          fontSize: "1.125rem",
                          marginBottom: "0.25rem",
                        }}
                      >
                        {wager.homeTeam} vs {wager.awayTeam}
                      </div>
                      <div
                        style={{
                          color: "#9CA3AF",
                          fontSize: "0.875rem",
                          marginBottom: "0.25rem",
                        }}
                      >
                        {wager.gameTime}
                      </div>
                      <div style={{ color: "#D1D5DB" }}>
                        {wager.description}
                      </div>
                    </div>
                  </div>
                  <div style={{ textAlign: "right" }}>
                    <div style={{ fontWeight: "bold", fontSize: "1.25rem" }}>
                      {wager.trackingOnly ? (
                        <span style={{ color: "#60A5FA" }}>Tracking Only</span>
                      ) : (
                        <span>
                          ${wager.amount} → ${wager.potentialPayout}
                        </span>
                      )}
                    </div>
                    <div style={{ color: "#9CA3AF" }}>
                      {wager.betType} • {wager.selection}
                    </div>
                    <div style={{ color: "#9CA3AF", fontSize: "0.875rem" }}>
                      Odds: {wager.odds > 0 ? "+" : ""}
                      {wager.odds}
                    </div>
                  </div>
                  <div
                    style={{
                      padding: "0.5rem 1rem",
                      borderRadius: "9999px",
                      fontSize: "0.875rem",
                      fontWeight: "bold",
                      backgroundColor:
                        wager.status === "Active" ? "#F59E0B" : "#10B981",
                      color: wager.status === "Active" ? "black" : "white",
                    }}
                  >
                    {wager.status}
                  </div>
                </div>
              </div>
            ))}
          </div>
        </div>
      </div>
    </div>
  );
};

export default WagerDashboard;
