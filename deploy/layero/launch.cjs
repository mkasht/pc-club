const { spawn } = require('node:child_process');
const { chmodSync } = require('node:fs');
const path = require('node:path');
const cwd = path.join(__dirname, 'app');
const executable = path.join(cwd, 'PcClub');
chmodSync(executable, 0o755);
const child = spawn(executable, [], {
    cwd,
    stdio: 'inherit',
    env: {
        ...process.env,
        ASPNETCORE_ENVIRONMENT: 'Production',
    },
});
for (const signal of ['SIGTERM', 'SIGINT']) process.on(signal, () => child.kill(signal));
child.on('error', (error) => {
    console.error(error.message);
    process.exit(1);
});
child.on('exit', (code) => process.exit(code ?? 1));
