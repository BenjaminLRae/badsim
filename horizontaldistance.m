%% eqn for horizontal distance

Vi = 40; % Vi = initial velocity, m/s
Angle = -20; % Angle = initial angle, deg

Vxi = Vi * cos(deg2rad(Angle)); % Vxi = initial horizontal velocity
Vyi = Vi * sin(deg2rad(Angle)); % Vyi = initial vertical velocity

Vt = 6.86; % Vt = terminal velocity, m/s

g = 9.81; % g = gravitational acceleration, m/s^2

t = 0:0.01:10;

lnint = (Vxi * g * t + (Vt^2))/(Vt^2);
x = ((Vt^2)/g)*log(lnint);

%% eqn for horizontal distance
lnint = sin(((g*t)/Vt)+atan(Vt/Vyi))/sin(atan(Vt/Vyi));
y = ((Vt^2)/g)*log(lnint);

y = y + 2.65;

%% cut to y > 0

for i = 1:length(y)
    if y(i) < 0
       finalIndex = i - 1;
       break;
    end
end

hold on;
plot(x(1:finalIndex),y(1:finalIndex));

