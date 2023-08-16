clear;

Vi = 80; % Vi = initial velocity, m/s
Angle = 30; % Angle = initial angle, deg

Vxi = Vi * cos(deg2rad(Angle)); % Vxi = initial horizontal velocity
Vyi = Vi * sin(deg2rad(Angle)); % Vyi = initial vertical velocity

Vt = 6.86; % Vt = terminal velocity, m/s

x = 0:0.1:20; % x = horizontal distance, from 0 to 20 in 0.1 m increments

g = 9.81; % g = gravitational acceleration, m/s^2

sin(atand(rad2deg(Vt/Vyi)))

%%
% AngleTest = -40:40;
% VxiTest = Vi * cos(deg2rad(AngleTest));
% VyiTest = Vi * sin(deg2rad(AngleTest));
% divisorTest = sin(deg2rad(atand(rad2deg(Vt./VyiTest))));
% 
% figure;
% plot(AngleTest,divisorTest);
% hold on;
% plot(AngleTest,VyiTest);

%%

divisor = sin((atan((Vt/Vyi))));
numerator = sin((Vt/Vxi)*(exp((g*x)/(Vt^2))-1) + atan((Vt/Vyi)));
% When the numerator becomes negative, the solution becomes complex and
% ceases to work. Need to remove negative values (or not calculate y for
% them, at least).

intclause = numerator/divisor;

y = ((Vt^2)/g)*log(numerator/divisor);  
y = y + 3;

for i = 1:length(y)
    if y(i) < 0
       finalIndex = i - 1; 
       break;
    end
end

%% eqn 13
%  logInt = (Vt * Vxi)./(Vt * Vxi - g * x);
%  y13 = x * ((Vt + Vyi)/Vxi) - ((Vt^2)/g) * log(logInt);


%%
hold on;
plot(x(1:finalIndex),y(1:finalIndex));
xlim([0 16]);
ylim([0 16]);

%%
%plot(x,y13);
%%
% for i = 1:length(numerator)
%    if numerator(i) < 0
%       finalindex = i - 1; 
%       break;
%    end
% end
% 
% for j = 1:finalindex    
%     y(j) = ((Vt^2)/g)*log(numerator(j)/divisor);    
% end
% 
% figure;
% plot(x(1:finalindex),y);