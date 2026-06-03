<script setup>
import { ref, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { getPlayers } from '../api';

const router = useRouter();
const players = ref([]);
const loading = ref(false);
const error = ref('');

async function loadPlayers() {
  loading.value = true;
  error.value = '';
  try {
    players.value = await getPlayers();
  } catch (err) {
    error.value = `Failed to load players: ${err.message}`;
  } finally {
    loading.value = false;
  }
}

function goToPlayer(playerId) {
  router.push(`/players/${playerId}`);
}

function normalizeRiskLevel(riskLevel) {
  return String(riskLevel || '').toLowerCase();
}

function riskBadgeClass(riskLevel) {
  const normalized = normalizeRiskLevel(riskLevel);

  if (normalized === 'low') return 'risk risk-low';
  if (normalized === 'medium') return 'risk risk-medium';
  if (normalized === 'high') return 'risk risk-high';
  if (normalized === 'very high') return 'risk risk-very-high';

  return 'risk risk-unknown';
}

onMounted(() => {
  loadPlayers();
});
</script>

<template>
  <section class="players-view">
    <header class="players-header">
      <h1>Players</h1>
      <p class="subtitle">Browse player profiles and quickly inspect risk and activity signals.</p>
      <p class="count" v-if="!loading && !error">{{ players.length }} players loaded</p>
    </header>

    <div v-if="error" class="error-alert">
      {{ error }}
    </div>

    <div v-if="loading" class="loading">Loading players...</div>

    <div v-else-if="players.length === 0" class="empty-state">
      <p>No players found.</p>
    </div>

    <div v-else class="players-grid">
      <article
        v-for="player in players"
        :key="player.id"
        class="player-card"
        @click="goToPlayer(player.playerId)"
        role="button"
        tabindex="0"
        @keydown.enter="goToPlayer(player.playerId)"
        @keydown.space.prevent="goToPlayer(player.playerId)"
      >
        <div class="card-top">
          <div>
            <p class="eyebrow">Player ID</p>
            <h2>{{ player.playerId }}</h2>
          </div>
          <span :class="riskBadgeClass(player.riskLevel)">{{ player.riskLevel }}</span>
        </div>

        <div class="info-group">
          <p><span class="label">Primary Sport</span><span class="value">{{ player.mostBetSport }}</span></p>
          <p><span class="label">Favourite Team</span><span class="value">{{ player.favouriteTeam }}</span></p>
        </div>

        <div class="metrics">
          <div class="metric">
            <span class="metric-label">Days Active</span>
            <span class="metric-value">{{ player.daysSinceJoined }}</span>
          </div>
          <div class="metric">
            <span class="metric-label">Risk Level</span>
            <span class="metric-value">{{ player.riskLevel }}</span>
          </div>
        </div>

        <button class="view-btn" @click.stop="goToPlayer(player.playerId)">View Details</button>
      </article>
    </div>
  </section>
</template>

<style scoped>
.players-view {
  padding: 16px 0 32px;
}

.players-header {
  margin-bottom: 20px;
  text-align: left;
}

.subtitle {
  margin: 4px 0 0;
  color: #4b5563;
  font-size: 16px;
}

.count {
  margin: 8px 0 0;
  font-size: 14px;
  color: #6b7280;
}

.loading,
.empty-state {
  padding: 24px;
  border: 1px solid #e5e7eb;
  border-radius: 12px;
  background: #ffffff;
  color: #6b7280;
  text-align: center;
}

.players-grid {
  display: grid;
  grid-template-columns: repeat(1, minmax(0, 1fr));
  gap: 16px;
}

.player-card {
  border: 1px solid #e5e7eb;
  border-radius: 14px;
  padding: 16px;
  background: #ffffff;
  cursor: pointer;
  transition: box-shadow 0.2s ease, transform 0.2s ease, border-color 0.2s ease;
  text-align: left;
}

.player-card:hover,
.player-card:focus-visible {
  box-shadow: 0 10px 24px rgba(0, 0, 0, 0.08);
  border-color: #cbd5e1;
  transform: translateY(-1px);
  outline: none;
}

.card-top {
  display: flex;
  align-items: flex-start;
  justify-content: space-between;
  gap: 12px;
  margin-bottom: 14px;
}

.eyebrow {
  margin: 0;
  color: #6b7280;
  font-size: 12px;
  letter-spacing: 0.04em;
  text-transform: uppercase;
}

h2 {
  margin: 4px 0 0;
}

.risk {
  border-radius: 999px;
  font-size: 12px;
  font-weight: 700;
  padding: 6px 10px;
  white-space: nowrap;
}

.risk-low {
  background: #dcfce7;
  color: #166534;
}

.risk-medium {
  background: #fef3c7;
  color: #92400e;
}

.risk-high,
.risk-very-high {
  background: #fee2e2;
  color: #991b1b;
}

.risk-unknown {
  background: #e5e7eb;
  color: #374151;
}

.info-group {
  border-top: 1px solid #f3f4f6;
  border-bottom: 1px solid #f3f4f6;
  padding: 10px 0;
  display: grid;
  gap: 8px;
}

.info-group p {
  display: flex;
  justify-content: space-between;
  gap: 12px;
}

.label {
  color: #6b7280;
  font-size: 13px;
}

.value {
  color: #111827;
  font-weight: 600;
  text-align: right;
}

.metrics {
  display: grid;
  grid-template-columns: repeat(2, minmax(0, 1fr));
  gap: 10px;
  margin: 12px 0 14px;
}

.metric {
  border: 1px solid #f3f4f6;
  border-radius: 10px;
  padding: 10px;
  background: #fafafa;
}

.metric-label {
  display: block;
  color: #6b7280;
  font-size: 12px;
  margin-bottom: 4px;
}

.metric-value {
  color: #111827;
  font-size: 18px;
  font-weight: 700;
}

.view-btn {
  width: 100%;
  border: 0;
  border-radius: 10px;
  padding: 10px 12px;
  font-size: 14px;
  font-weight: 700;
  color: #ffffff;
  background: #2563eb;
  cursor: pointer;
}

.view-btn:hover {
  background: #1d4ed8;
}

@media (min-width: 768px) {
  .players-grid {
    grid-template-columns: repeat(2, minmax(0, 1fr));
  }
}

@media (min-width: 1100px) {
  .players-grid {
    grid-template-columns: repeat(3, minmax(0, 1fr));
  }
}
</style>
