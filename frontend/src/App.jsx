import { useEffect, useState } from "react";

const cards = [
  {
    title: "ASP.NET backend",
    description: "A Web API powered by .NET 8 that serves JSON for the React app."
  },
  {
    title: "React frontend",
    description: "A modern React UI built with Vite and connected through a dev proxy."
  },
  {
    title: "Ready to extend",
    description: "Add authentication, data access, or more endpoints as you scale."
  }
];

export default function App() {
  const [status, setStatus] = useState({
    loading: true,
    error: null,
    data: null
  });

  useEffect(() => {
    let cancelled = false;

    const loadStatus = async () => {
      try {
        const response = await fetch("/api/status");
        if (!response.ok) {
          throw new Error("Unable to reach backend API.");
        }
        const data = await response.json();
        if (!cancelled) {
          setStatus({ loading: false, error: null, data });
        }
      } catch (error) {
        if (!cancelled) {
          setStatus({ loading: false, error: error.message, data: null });
        }
      }
    };

    loadStatus();

    return () => {
      cancelled = true;
    };
  }, []);

  return (
    <div className="page">
      <header className="hero">
        <p className="eyebrow">React + ASP.NET Core</p>
        <h1>Net T Y B</h1>
        <p className="lead">
          The frontend remains React, while the backend is now an ASP.NET Core Web API.
        </p>
        <div className="status">
          {status.loading && <span>Checking backend status...</span>}
          {status.error && (
            <span className="error">{status.error}</span>
          )}
          {status.data && (
            <span className="success">
              {status.data.name}: {status.data.message}
            </span>
          )}
        </div>
        <div className="actions">
          <a className="primary" href="https://learn.microsoft.com/aspnet/core">
            ASP.NET Core docs
          </a>
          <a className="secondary" href="https://react.dev">
            React docs
          </a>
        </div>
      </header>
      <section className="grid">
        {cards.map((card) => (
          <article key={card.title} className="card">
            <h2>{card.title}</h2>
            <p>{card.description}</p>
          </article>
        ))}
      </section>
    </div>
  );
}
